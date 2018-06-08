﻿using System;
using System.Linq;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;

namespace Toggl.Ultrawave.Exceptions
{
    public class ApiException : Exception
    {
        private const string badJsonLocalisedError = "Encountered unexpected error.";

        private readonly string message;
        internal IRequest Request { get; }
        internal IResponse Response { get; }

        public string LocalizedApiErrorMessage { get; }
        public override string Message => ToString();

        internal ApiException(IRequest request, IResponse response, string defaultMessage)
        {
            message = defaultMessage;
            Request = request;
            Response = response;
            LocalizedApiErrorMessage = getLocalizedMessageFromResponse(response);
        }

        #if DEBUG
        public override string ToString()
            => $"{GetType().Name} ({message}) for request {Request.HttpMethod} {Request.Endpoint} "
               + $"with response {serialisedResponse}";
        #else
        public override string ToString()
            => $"{GetType().Name} ({message}) for request {Request.HttpMethod} {Request.Endpoint}";
        #endif

        private string serialisedResponse => new JsonSerializer().Serialize(
            new
            {
                Status = $"{(int)Response.StatusCode} {Response.StatusCode}",
                Headers = Response.Headers.ToDictionary(h => h.Key, h => h.Value),
                Body = Response.RawData
            });


        private static string getLocalizedMessageFromResponse(IResponse response)
        {
            if (!response.IsJson)
                return response.RawData;

            try
            {
                var error = new JsonSerializer().Deserialize<ResponseError>(response.RawData);
                return error?.Message ?? badJsonLocalisedError;
            }
            catch (DeserializationException)
            {
                return badJsonLocalisedError;
            }
        }
    }
}
