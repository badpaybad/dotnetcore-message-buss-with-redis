using Microsoft.Extensions.Configuration;
using RedisUsage.CqrsCore.Ef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace RedisUsage.CqrsCore.RegisterEngine
{
    public static class CommandsAndEventsRegisterEngine
    {
        static Dictionary<string, Type> _cmdAndEvtTypeFullname = new Dictionary<string, Type>();

        static CommandsAndEventsRegisterEngine()
        {
            //try
            //{
            //    using (var db = new CommandEventStorageDbContext())
            //    {
            //        db.Database.EnsureCreated();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message + $"Can not connect to db {nameof(CommandEventStorageDbContext)}", ex);
            //}
        }

        /// <summary>
        /// Build setting from appsettings.json
        /// </summary>
        /// <param name="appSettingsFileName"></param>
        public static void Init(string appSettingsFileName = "appsettings.json")
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFileName, true, true).Build();

            ConfigurationManagerExtensions.SetConfiguration(config);

            var redisHost = ConfigurationManagerExtensions.GetValueByKey("Redis:Host") ?? "127.0.0.1";
            var redisPort = ConfigurationManagerExtensions.GetValueByKey("Redis:Port") ?? "6379";
            var redisPwd = ConfigurationManagerExtensions.GetValueByKey("Redis:Password") ?? string.Empty;
            int? redisPortInt = null;
            if (!string.IsNullOrEmpty(redisPort))
            {
                redisPortInt = int.Parse(redisPort);
            }

            RedisUsage.RedisServices.RedisServices.Init(redisHost, redisPortInt, redisPwd);
        }

        /// <summary>
        /// Consumer register
        /// </summary>
        /// <returns></returns>
        public static bool AutoRegisterForHandlers()
        {
            List<Assembly> allAss = FindAllDll();

            foreach (var assembly in allAss)
            {
                try
                {
                    RegisterAssemblyForHandlers(assembly);
                }
                catch (Exception)
                {
                    // Console.WriteLine("Can not register assembly: " + assembly.FullName);
                    //Console.WriteLine("- " + ex.GetAllMessages());
                }
            }

            return true;
        }

        public static object TryFindType(string typeFullName, out object foundType)
        {
            throw new NotImplementedException();
        }

        private static List<Assembly> FindAllDll()
        {
            // return AppDomain.CurrentDomain.GetAssemblies();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            List<Assembly> allAssemblies = new List<Assembly>();

            allAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

            var dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

            foreach (string dll in dllFiles)
            {
                if (File.Exists(dll))
                {
                    Assembly assbl;

                    try
                    {
                        //allAssemblies.Add(Assembly.LoadFile(dll));
                        assbl = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    }
                    catch (Exception)
                    {
                        //Console.WriteLine(ex.GetAllMessages() + "\r\n" + "Can not load dll: " + dll);
                        assbl = Assembly.LoadFile(dll);
                    }

                    if (assbl != null)
                    {
                        allAssemblies.Add(assbl);
                    }
                }
            }
            // return allAssemblies;

            var rescanDlls = AppDomain.CurrentDomain.GetAssemblies();

            allAssemblies.AddRange(rescanDlls);

            Dictionary<string, Assembly> filter = new Dictionary<string, Assembly>();

            foreach (var a in allAssemblies)
            {
                filter[a.FullName] = a;
            }

            return filter.Values.ToList();
        }

        /// <summary>
        /// Register consummer
        /// </summary>
        /// <param name="executingAssembly"></param>
        public static void RegisterAssemblyForHandlers(Assembly executingAssembly)
        {
            var allTypes = executingAssembly.GetTypes();

            var listHandler = allTypes.Where(t => typeof(ICqrsHandle).IsAssignableFrom(t)
                                                  && t.IsClass && !t.IsAbstract).ToList();

            var listCmdsEvts= allTypes.Where(t => (typeof(ICommand).IsAssignableFrom(t) || typeof(IEvent).IsAssignableFrom(t))
                                                   && t.IsClass && !t.IsAbstract).ToList();

            foreach(var pParameterType in listCmdsEvts)
            {
                RegisterCommandOrEventType(pParameterType);
            }
           
            var assemblyFullName = executingAssembly.FullName;
            if (listHandler.Count <= 0)
            {
                //Console.WriteLine("Not found ICqrsHandle in " + assemblyFullName);
                return;
            }

            Console.WriteLine($"+-{assemblyFullName}");
            //Console.WriteLine($"Found {listHandler.Count} handle(s) to register to message buss");

            foreach (var handlerType in listHandler)
            {
                var cqrsHandler = (ICqrsHandle)Activator.CreateInstance(handlerType);
                if (cqrsHandler == null) continue;

                Console.WriteLine($"Found ICqrsHandle type: {cqrsHandler.GetType()}");

                MethodInfo[] allMethod = cqrsHandler.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var mi in allMethod)
                {
                    var methodName = mi.Name;
                    if (!methodName.Equals("handle", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var pParameterType = mi.GetParameters().SingleOrDefault().ParameterType;

                    if (typeof(IEvent).IsAssignableFrom(pParameterType))
                    {
                        var typeFullAssemblyQualifiedName = pParameterType.AssemblyQualifiedName;
                       
                        var className = mi.DeclaringType.FullName;

                        RedisUsage.RedisServices.MessageBussServices.Subscribe($"{className}_{typeFullAssemblyQualifiedName}", typeFullAssemblyQualifiedName, (o) =>
                        {
                            mi.Invoke(cqrsHandler, new object[] { o });
                        });

                        Console.WriteLine($"Regsitered method to process Event type: {pParameterType}");
                    }

                    if (typeof(ICommand).IsAssignableFrom(pParameterType))
                    {
                        var typeFullAssemblyQualifiedName = pParameterType.AssemblyQualifiedName;
                                            
                        var className = mi.DeclaringType.FullName;

                        RedisUsage.RedisServices.MessageBussServices.Subscribe($"{className}_{typeFullAssemblyQualifiedName}", typeFullAssemblyQualifiedName, (o) =>
                        {
                            mi.Invoke(cqrsHandler, new object[] { o });
                        });

                        Console.WriteLine($"Regsitered method to process Command type: {pParameterType}");
                    }
                }
            }
        }

        private static void RegisterCommandOrEventType(Type pParameterType)
        {
            lock (_cmdAndEvtTypeFullname)
            {
                var tfn = pParameterType.FullName;
                if (_cmdAndEvtTypeFullname.ContainsKey(tfn) == false)
                {
                    _cmdAndEvtTypeFullname[tfn] = pParameterType;
                }
            }
        }

        public static bool TryFindType(string typeFullName, out Type type)
        {
            type = null;

            lock (_cmdAndEvtTypeFullname)
            {
                if (_cmdAndEvtTypeFullname.TryGetValue(typeFullName, out type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
