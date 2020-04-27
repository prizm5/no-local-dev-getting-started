using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Afirmon_Webapp {
  public class ErrorResponse  { 
    public List<string> errors {get; set;} = new List<string>();

    public static async Task SendErrorResponse(HttpResponse response, String errorMsg) {
      var error = new ErrorResponse{errors = {errorMsg}};
      await response.WriteAsync(JsonConvert.SerializeObject(error));
    }
  }
}