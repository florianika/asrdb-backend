﻿

namespace Application.Rule.ChangeRuleStatus.Request
{
    public class ChangeRuleStatusRequest : Rule.Request
    {
        public long Id { get; set; }
        public Guid UpdatedUser { get; set; }
    }
}
