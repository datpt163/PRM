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
    public class UpdateDefaultLabelCommandHandle : IRequestHandler<UpdateDefaultLabelCommand, ResponseMediator>
    {
        private readonly IFileService _fileService;

        public UpdateDefaultLabelCommandHandle(IFileService fileService)
        {
            _fileService = fileService;
        }
        public async Task<ResponseMediator> Handle(UpdateDefaultLabelCommand request, CancellationToken cancellationToken)
        {
            string module = "Module";
            string project = "Projects";
            string folder = "Default";
            string fileName = "DefaultLabel.json";
            string path = Path.Combine(module, project, folder, fileName);
            var labels = JsonSerializer.Deserialize<List<Domain.Entities.Label>>(await _fileService.ReadFileAsync(path)) ?? new List<Domain.Entities.Label>();
            var label = labels.FirstOrDefault(x => x.Id == request.Id);
            if (label == null)
                return new ResponseMediator(Messages.label_not_found, null, 404);
            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator(Messages.title_empty, null, 400);

            var labelCheckDuplicateTitle = labels.FirstOrDefault(x => x.Id != request.Id && x.Title.Trim().ToUpper() == request.Title.Trim().ToUpper());
            if (labelCheckDuplicateTitle != null)
                return new ResponseMediator(Messages.title_label_available, null, 400);

            label.Title = request.Title;
            label.Description = request.Description;
            await _fileService.WriteFileAsync(path, JsonSerializer.Serialize(labels));
            return new ResponseMediator("", label);
        }
    }
}
