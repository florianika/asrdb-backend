﻿namespace Application.User.SignOut.Response
{
    public class SignOutErrorResponse : SignOutResponse
    {
        public string? Message { get; internal set; }
        public string? Code { get; internal set; }
    }
}
