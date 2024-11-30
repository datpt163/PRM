using Capstone.Application.Common.Cloudinaries;
using Capstone.Application.Module.Applicants.Command;
using Capstone.Application.Module.Applicants.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Helpers;
using Capstone.Infrastructure.Helpers;
using Capstone.Infrastructure.Repository;
using MediatR;


namespace Capstone.Application.Module.Applicants.CommandHandler
{
    public class AddApplicantCommandHandler : IRequestHandler<AddApplicantCommand, ApplicantDto>
    {
        private readonly IRepository<Applicant> _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public AddApplicantCommandHandler(IRepository<Applicant> applicantRepository, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _applicantRepository = applicantRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ApplicantDto> Handle(AddApplicantCommand command, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(command.Email))
            {
                if (!EmailHelper.IsValidEmail(command.Email))
                {
                    throw new ArgumentException("Invalid email format.");
                }
            }
            if (!string.IsNullOrEmpty(command.PhoneNumber))
            {
                if (!PhoneNumberValidator.Validate(command.PhoneNumber))
                {
                    throw new ArgumentException("Invalid phone number format.");
                }

            }
            string? cvUrl = null;
            if (command.CvFile != null)
            {
                using var stream = new MemoryStream();
                await command.CvFile.CopyToAsync(stream);
                stream.Position = 0;
                cvUrl = await _cloudinaryService.UploadPdfAsync(stream, command.CvFile.FileName);
            }

            var applicant = new Applicant
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Email = command.Email,
                StartDate = command.StartDate,
                PhoneNumber = command.PhoneNumber,
                CvLink = cvUrl,
                CreatedAt = DateTime.Now,
            };

             _applicantRepository.Add(applicant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApplicantDto
            {
                Id = applicant.Id,
                Name = applicant.Name,
                Email = applicant.Email,
                StartDate = applicant.StartDate,
                PhoneNumber = applicant.PhoneNumber,
                CvLink = applicant.CvLink,
                CreatedAt = applicant.CreatedAt,
                CreatedBy = applicant.CreatedBy,
                UpdatedAt = applicant.UpdatedAt,
                UpdatedBy = applicant.UpdatedBy,
                IsDeleted = applicant.IsDeleted,
            };
        }
    }
}
