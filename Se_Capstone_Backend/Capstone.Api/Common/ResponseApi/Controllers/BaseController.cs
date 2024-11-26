using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Application.Common.Paging;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Capstone.Api.Common.ResponseApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult ResponseOk(dynamic? dataResponse = null, string message = "", bool overrideBody = true)
        {
            return StatusCode((int)HttpStatusCode.OK, overrideBody ? new { data = dataResponse, Message = message, statusCode = (int)HttpStatusCode.OK } : dataResponse);
        }
        protected IActionResult ResponseOk<T>(IEnumerable<T> data, PagingSP? paging, string message = "")
        {
            var response = new
            {
                Data = data,
                Meta = paging,
                Message = message,
                statusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, response);
        }

        protected IActionResult ResponseOk<T>(IEnumerable<T> data, PagingSP? paging, int unReadCount, string message = "")
        {
            var response = new
            {
                Data = data,
                Meta = paging,
                Message = message,
                statusCode = (int)HttpStatusCode.OK,
                UnReadCount = unReadCount
            };

            return StatusCode((int)HttpStatusCode.OK, response);
        }

        protected IActionResult ResponseCreated(dynamic? dataResponse = null, string? messageResponse = null)
        {
            return StatusCode((int)HttpStatusCode.Created, new { data = dataResponse, message = messageResponse, statusCode = (int)HttpStatusCode.Created });
        }

        protected IActionResult ResponseNoContent()
        {
            return StatusCode((int)HttpStatusCode.NoContent);
        }

        protected IActionResult ResponseBadRequest(string? messageResponse = null, dynamic? dataResponse = null, bool overrideBody = true)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, overrideBody ? new ResponseFail() { Message = messageResponse, StatusCode = (int)HttpStatusCode.BadRequest } : dataResponse);
        }

        protected IActionResult ResponseUnauthorized(string? messageResponse = null)
        {
            return StatusCode((int)HttpStatusCode.Unauthorized, new ResponseFail() { Message = messageResponse, StatusCode = (int)HttpStatusCode.Unauthorized });
        }

        protected IActionResult ResponseForbidden(string? messageResponse = null, int codeResponse = (int)HttpStatusCode.Forbidden)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, new { message = messageResponse, code = codeResponse, statusCode = (int)HttpStatusCode.Forbidden });
        }

        protected IActionResult ResponseInternalServerError(string? messageResponse = null, int codeResponse = (int)HttpStatusCode.InternalServerError)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = messageResponse, code = codeResponse, statusCode = (int)HttpStatusCode.InternalServerError });
        }
        protected IActionResult ResponseNotFound(string? messageResponse = null)
        {
            return StatusCode((int)HttpStatusCode.NotFound, new ResponseFail() { Message = messageResponse, StatusCode = (int)HttpStatusCode.NotFound });
        }
    }
}
