using Capstone.Application.Module.Skills.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Skills.Query
{
    public class GetUsersBySkillTitleQuery : IRequest<List<UsersSkillDto>>
    {
        public string SkillTitle { get; set; }
    }
}
