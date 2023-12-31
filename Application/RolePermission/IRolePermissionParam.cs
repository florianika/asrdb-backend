﻿
namespace Application.RolePermission
{
    public interface IRolePermissionParam<Req, Res>
        where Req : RequestRolePermission
        where Res : ResponseRolePermission
    {
        public Task<Res> Execute(Req request);
    }
}
