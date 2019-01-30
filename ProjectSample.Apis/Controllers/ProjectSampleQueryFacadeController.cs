using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSample.Database;
using ProjectSample.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSample.Apis.Controllers
{
    [Route("api/ProjectSampleQueryFacade")]
    [ApiController]
    [Produces("application/json")]
    public class ProjectSampleQueryFacadeController : ControllerBase
    {
        //[Authorize]
        [Route("GetDetail")]
        public object GetDetail(Guid id)
        {
            Sample result;
            using (var db = new ProjectSampleDatabase())
            {

                result = db.Samples.Where(i => i.Id == id)
                   .SingleOrDefault();

            }

            result = result ?? new Sample();

            Sample innerSample = new Sample();
            if (!string.IsNullOrEmpty(result.JsonData))
            {
                innerSample = JsonConvert.DeserializeObject<Sample>(result.JsonData);
            }

            return new
            {
                Id = result.Id,
                Version = result.Version,
                InnerData = innerSample
            };
        }

        [Route("GetPaging")]
        public List<object> GetPaging(int? skip = 0, int? take = 10)
        {
            int xskip = 0;
            if (skip != null) xskip = skip.Value;

            int xtake = 0;
            if (take != null) xtake = take.Value;

            List<Sample> result;

            using (var db = new ProjectSampleDatabase())
            {
                result = db.Samples.Skip(xskip).Take(xtake)
                                     .ToList();
            }

            result = result ?? new List<Sample>();

            List<object> temp = new List<object>();

            foreach (var r in result)
            {
                if (!string.IsNullOrEmpty(r.JsonData))
                {
                    temp.Add(new
                    {
                        Id = r.Id,
                        Version = r.Version
                    });
                }
                else
                {
                    temp.Add(new
                    {
                        Id = r.Id,
                        Version = r.Version
                    });
                }

            }

            return temp;
        }
    }
}
