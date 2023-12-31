﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityGuide.API.Helpers
{
    public static class JwtExtension
    {
        public static void AddAplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Allow-Origin","*");
            response.Headers.Add("Access-Contol-Expose-Header","Application-Error"); 
        }
    }
}
