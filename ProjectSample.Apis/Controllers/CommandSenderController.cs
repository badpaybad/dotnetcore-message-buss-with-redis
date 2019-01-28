using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSample.CommandsAndEvents;
using RedisUsage.CqrsCore;
using RedisUsage.CqrsCore.Ddd;
using RedisUsage.CqrsCore.Extensions;
using RedisUsage.CqrsCore.RegisterEngine;

namespace ProjectSample.Apis.Controllers
{
    [Route("api/CommandSender")]
    [ApiController]
    [Produces("application/json")]
    public class CommandSenderController : ControllerBase
    {
        [HttpPost]
        //[Authorize]
        public CommandResponse Post(CommandRequest cmd)
        {
            try
            {
                var jobj = JsonConvert.DeserializeObject(cmd.CommandDataJson) as Newtonsoft.Json.Linq.JObject;

                Type foundType;

                var found = CommandsAndEventsRegisterEngine.TryFindCommandOrEventType(cmd.CommandTypeFullName, out foundType);

                if (!found)
                {
                    return new CommandResponse()
                    {
                        Success = false,
                        StatusCode = HttpStatusCode.NotImplemented,
                        Message = "Not found command type",
                        CommandId = Guid.Empty
                    };
                }

                var ocmd = (ICommand)jobj.ToObject(foundType);

                CommandPublisher.Instance.Send(ocmd);

                return new CommandResponse()
                {
                    Success = true,
                    CommandId = ocmd.PublishedCommandId.Value,
                    Message = "Success",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new CommandResponse()
                {
                    CommandId = Guid.Empty,
                    Message = ex.GetAllMessages(),
                    StatusCode = HttpStatusCode.BadGateway,
                    Success = false
                };
            }

        }

        public string Get(string typeFullName)
        {
            string cmdTemp = JsonConvert.SerializeObject(new CreateSample(Guid.NewGuid(), "V" + DateTime.Now.GetHashCode(), "{}"));

            var request = new CommandRequest();

            request.CommandTypeFullName = typeof(CreateSample).FullName;
            request.CommandDataJson = cmdTemp;

            if (string.IsNullOrEmpty(typeFullName))
            {
                return JsonConvert.SerializeObject(request);
            }

            Type foundType;

            var found = CommandsAndEventsRegisterEngine.TryFindCommandOrEventType(typeFullName, out foundType);

            if (!found)
            {
                return JsonConvert.SerializeObject(request);
            }

            cmdTemp = JsonConvert.SerializeObject(Activator.CreateInstance(foundType));

            request.CommandTypeFullName = foundType.FullName;
            request.CommandDataJson = cmdTemp;

            return JsonConvert.SerializeObject(request);
        }
    }

    public class CommandRequest
    {
        public string CommandTypeFullName { get; set; }
        public string CommandDataJson { get; set; }
    }

    public class CommandResponse
    {
        public Guid CommandId { get; set; }

        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }
    }
}
