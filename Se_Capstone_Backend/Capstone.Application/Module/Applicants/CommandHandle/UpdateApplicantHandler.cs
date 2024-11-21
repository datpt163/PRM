using Capstone.Application.Common.Cloudinaries;
using Capstone.Application.Module.Applicants.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Helpers;
using Capstone.Infrastructure.Helpers;
using Capstone.Infrastructure.Repository;
using MediatR;


public class UpdateApplicantCommandHandler : IRequestHandler<UpdateApplicantCommand, ApplicantDto?>
{
    private readonly IRepository<Applicant> _applicantRepository;
    private readonly CloudinaryService _cloudinaryService;
    private readonly IRepository<Job> _jobRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateApplicantCommandHandler(IRepository<Applicant> applicantRepository, IRepository<Job> jobRepository, CloudinaryService cloudinaryService, IIdentityService identityService, IUnitOfWork unitOfWork)
    {
        _applicantRepository = applicantRepository;
        _jobRepository = jobRepository;
        _cloudinaryService = cloudinaryService;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplicantDto?> Handle(UpdateApplicantCommand request, CancellationToken cancellationToken)
    {
        var applicant = await _applicantRepository.FindOneAsync(a => a.Id == request.Id);

        if (applicant == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            if (!EmailHelper.IsValidEmail(request.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            if (!PhoneNumberValidator.Validate(request.PhoneNumber))
            {
                throw new ArgumentException("Invalid phone number format.");
            }

        }
        applicant.Name = request.Name;
        applicant.Email = request.Email;
        applicant.StartDate = request.StartDate;
        applicant.PhoneNumber = request.PhoneNumber;

        if (request.CvFile != null)
        {
            if (!string.IsNullOrEmpty(applicant.CvLink))
            {
                await _cloudinaryService.DeletePdfByUrlAsync(applicant.CvLink);
            }

            using var pdfStream = new MemoryStream();
            await request.CvFile.CopyToAsync(pdfStream);
            pdfStream.Position = 0;
            applicant.CvLink = await _cloudinaryService.UploadPdfAsync(pdfStream, request.CvFile.FileName);
        }

        applicant.MainJobId = request.MainJobId;
        applicant.MainJob = await _jobRepository.FindOneAsync(x => x.Id == request.MainJobId);
        applicant.IsOnBoard = request.IsOnBoard;

        if (request.JobIds != null && request.JobIds.Any())
        {
            applicant.Jobs.Clear(); 
            foreach (var jobId in request.JobIds)
            {
                var job = await _jobRepository.FindOneAsync(x => x.Id == jobId);
                if (job != null)
                {
                    applicant.Jobs.Add(job); 
                }
            }
        }
        applicant.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        await _identityService.SetUpdatedByAsync(applicant);
        await _applicantRepository.UpdateAsync(applicant);
        await _unitOfWork.SaveChangesAsync();
        var applicantDto = new ApplicantDto
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

        return applicantDto;
    }
}
