using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ZB_ProfileDataAccess;

namespace ZB_ProfileWebAPI.Controllers
{
    public class UserController : ApiController
    {
        [Authorize]
        public IEnumerable<AspNetUser> Get()
        {
            using (ZB_Profile_DBEntities entities = new ZB_Profile_DBEntities())
            {
                return entities.AspNetUsers.ToList();
            }
        }

        public HttpResponseMessage Get(string id)
        {
            using (ZB_Profile_DBEntities entities = new ZB_Profile_DBEntities())
            {
                var entity = entities.AspNetUsers.FirstOrDefault(c => c.Id == id);
                if (entity != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User with ID: " + id.ToString() + "not found");
                }
            }
        }
        public HttpResponseMessage Post([FromBody] AspNetUser user)
        {
            try
            {
                using (ZB_Profile_DBEntities entities = new ZB_Profile_DBEntities())
                {
                    entities.AspNetUsers.Add(user);
                    entities.SaveChanges();
                    var message = Request.CreateResponse(HttpStatusCode.Created, user);
                    message.Headers.Location = new Uri(Request.RequestUri + user.Id.ToString());
                    return message;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        public HttpResponseMessage Delete(string id)
        {
            try
            {
                using (ZB_Profile_DBEntities entities = new ZB_Profile_DBEntities())
                {
                    var entity = entities.AspNetUsers.FirstOrDefault(c => c.Id == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with ID: " + id.ToString() + "not found");
                    }
                    else {
                        entities.AspNetUsers.Remove(entity);
                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        public HttpResponseMessage Put(string id, [FromBody] AspNetUser user)
        {
            try
            {
                using (ZB_Profile_DBEntities entities = new ZB_Profile_DBEntities())
                {
                    var entity = entities.AspNetUsers.FirstOrDefault(c => c.Id == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with ID: " + id.ToString() + "not found");
                    }
                    else {

                        entity.UserName = user.UserName;

                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
