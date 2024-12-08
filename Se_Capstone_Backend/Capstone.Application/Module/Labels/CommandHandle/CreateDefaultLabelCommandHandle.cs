using Capstone.Application.Common.FileService;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Labels.Command;
using Capstone.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Labels.CommandHandle
{
    public class CreateDefaultLabelCommandHandle : IRequestHandler<CreateLabelDefaultCommand, ResponseMediator>
    {
        private readonly IFileService _fileService;

        public CreateDefaultLabelCommandHandle(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<ResponseMediator> Handle(CreateLabelDefaultCommand request, CancellationToken cancellationToken)
        {
            string module = "Module";
            string project = "Projects";
            string folder = "Default";
            string fileName = "DefaultLabel.json";
            string path = Path.Combine(module, project, folder, fileName);
            var labels = JsonSerializer.Deserialize<List<Domain.Entities.Label>>(await _fileService.ReadFileAsync(path)) ?? new List<Domain.Entities.Label>();
            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator(Messages.title_empty, null, 400);

            if (labels.Select(x => x.Title.Trim().ToUpper()).Contains(request.Title.Trim().ToUpper()))
                return new ResponseMediator(Messages.title_label_available, null, 400);

            var label = new Capstone.Domain.Entities.Label() { Id = Guid.NewGuid(), Title = request.Title, Description = request.Description };
            labels.Add(label);
            await _fileService.WriteFileAsync(path, JsonSerializer.Serialize(labels));
            return new ResponseMediator("", new { ID = label.Id, Title = label.Title, Description = request.Description});
        }
    }
}
