﻿
namespace Application.User.RefreshToken.Response
{
    public class RefreshTokenErrorResponse : RefreshTokenResponse
    {
        public string? Message { get; internal set; }
        public string? Code { get; internal set; }
    }
}
