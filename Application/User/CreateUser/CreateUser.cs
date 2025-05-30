﻿using Application.Ports;
using Application.User.CreateUser.Request;
using Application.User.CreateUser.Response;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using Claim = Domain.Claim;

namespace Application.User.CreateUser
{
    public class CreateUser : ICreateUser
    {
        private readonly ILogger _logger;
        private readonly ICryptographyService _cryptographyService;
        private readonly IAuthRepository _authRepository;
        public CreateUser(ILogger<CreateUser> logger, ICryptographyService cryptographyService, IAuthRepository authRepository)
        {
            _logger = logger;
            _cryptographyService = cryptographyService;
            _authRepository = authRepository;
        }

        public virtual async Task<CreateUserResponse> Execute(CreateUserRequest request)
        {
            try
            {
                var salt = _cryptographyService.GenerateSalt();
                var currentDate = DateTime.Now;
                var refreshToken = new Domain.RefreshToken
                {
                    Value = "Empty",
                    Active = false,
                    ExpirationDate = currentDate,
                };

                var userId = Guid.NewGuid();

                var user = new Domain.User
                {
                    Id = userId,
                    Active = true,
                    Email = request.Email,
                    Password = _cryptographyService.HashPassword(request.Password, salt),
                    Salt = salt,
                    Name = request.Name,
                    LastName = request.LastName,
                    Claims = ToClaims(userId, request.MunicipalityCode),
                    CreationDate = currentDate,
                    UpdateDate = currentDate,
                    RefreshToken = refreshToken,
                    AccountStatus = AccountStatus.ACTIVE, //Creating user with Active Status
                    AccountRole = AccountRole.USER, //Creating user with the role User
                    SigninFails = 0, //while creating a new user, settings the value to 0
                    LockExpiration = null //setting lockexpiration to null                    
                };

                 await _authRepository.CreateUser(user);

                return new CreateUserSuccessResponse
                {
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private static List<Claim>? ToClaims(Guid userId, string municipality)
        {
            var claims = new List<Claim>
                {
                    new() { UserId = userId, Type = nameof(municipality), Value = municipality }
                };
            return claims;
        }
    }
}
