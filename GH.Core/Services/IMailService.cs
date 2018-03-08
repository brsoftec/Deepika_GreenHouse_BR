using GH.Core.ViewModels;
using System.Threading.Tasks;

namespace GH.Core.Services
{
    public interface IMailService
    {
        Task SendMailAsync(NotificationContent email);
        Task SendMailAttachmentAsync(NotificationContent email, string filePath, string contentHtml);
        Task SendMailBCCAsync(NotificationContent email, string[] listBCC);

    }
}