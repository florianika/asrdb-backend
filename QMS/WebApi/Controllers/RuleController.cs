﻿using Application.Rule.CreateRule;
using Application.Rule.CreateRule.Request;
using Application.Rule.CreateRule.Response;
using Application.Rule.GetAllRules;
using Application.Rule.GetAllRules.Response;
using Application.Rule.GetRule;
using Application.Rule.GetRule.Request;
using Application.Rule.GetRule.Response;
using Application.Rule.GetRulesByEntity;
using Application.Rule.GetRulesByEntity.Request;
using Application.Rule.GetRulesByEntity.Response;
using Application.Rule.GetRulesByQualityAction.Request;
using Application.Rule.GetRulesByVariableAndEntity;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using Application.Rule.GetRulesByVariableAndEntity.Response;
using Application.Rule.GetRulesByVariableAndEntity.Request;
using Application.Rule.GetRulesByQualityAction;
using Application.Rule.GetRulesByQualityAction.Response;
using Application.Rule.ChangeRuleStatus;
using Application.Rule.ChangeRuleStatus.Response;
using Application.Rule.ChangeRuleStatus.Request;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/qms/rules")]
    public class RuleController : ControllerBase
    {
        private readonly CreateRule _createRuleService;
        private readonly GetAllRules _getAllRulesService;
        private readonly GetRulesByVariableAndEntity _getRulesByVariableAndEntityService;
        private readonly GetRulesByEntity _getRulesByEntityService;
        private readonly GetRule _getRuleService;
        private readonly GetRulesByQualityAction _getRulesByQualityActionService;
        private readonly ChangeRuleStatus _changeRuleStatusService;
        public RuleController(CreateRule createRuleService, GetAllRules getAllRulesService,
             GetRulesByVariableAndEntity getRulesByVariableAndEntityService, GetRulesByEntity getRulesByEntity, 
             GetRule getRuleService, GetRulesByQualityAction getRulesByQualityActionService,
             ChangeRuleStatus changeRuleStatusService)
        {
            _createRuleService = createRuleService;
            _getAllRulesService = getAllRulesService;
            _getRulesByVariableAndEntityService = getRulesByVariableAndEntityService;
            _getRulesByEntityService = getRulesByEntity;
            _getRuleService = getRuleService;
            _getRulesByQualityActionService = getRulesByQualityActionService;
            _changeRuleStatusService = changeRuleStatusService;
        }
        [HttpPost]
        [Route("")]
        public async Task<CreateRuleResponse> CreateRule(CreateRuleRequest request)
        {   //todo - store who created the rule
            return await _createRuleService.Execute(request);
        }
        [HttpGet]
        [Route("")]
        public async Task<GetAllRulesResponse> GetAllRules()
        {
            return await _getAllRulesService.Execute();
        }
        [HttpGet]
        [Route("variable/{variable}/type/{entityType}")]
        public async Task<GetRulesByVariableAndEntityResponse> GetRulesByVarialeAndEntityType(string variable, EntityType entityType)
        {
            return await _getRulesByVariableAndEntityService.Execute(new GetRulesByVariableAndEntityRequest() { Variable = variable, EntityType = entityType });
        }

        [HttpGet]
        [Route("entity/{entityType}")]
        public async Task<GetRulesByEntityResponse> GetRulesByEntity(EntityType entityType)
        {
            return await _getRulesByEntityService.Execute(new GetRulesByEntityRequest() { EntityType = entityType });
        }
        [HttpGet]
        [Route("/{id}")]
        public async Task<GetRuleResponse> GetRule(long id)
        {
            return await _getRuleService.Execute(new GetRuleRequest() { Id = id });
        }
        [HttpGet]
        [Route("qualityAction/{qualityAction}")]
        public async Task<GetRulesByQualityActionResponse> GetRulesByQualityAction(QualityAction qualityAction)
        {
            return await _getRulesByQualityActionService.Execute(new GetRulesByQualityActionRequest() { QualityAction = qualityAction });
        }

        [HttpPatch]
        [Route("/{id}")]
        public async Task<ChangeRuleStatusResponse> ChangeRuleStatus(long id)
        {
            //todo - store who changed the status
            return await _changeRuleStatusService.Execute(new ChangeRuleStatusRequest() { Id = id});
        }
    }
}
