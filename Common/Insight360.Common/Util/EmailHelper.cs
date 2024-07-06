using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Data;

namespace Insight360.Common
{
    public class EmailHelper
    {
        public string SMTPHost { get; private set; }
        public int SMTPPort { get; private set; }
        public string SMTPUserName { get; private set; }
        public string SMTPPassword { get; private set; }
        public List<Attachment> Attachments { get; private set; }

        public EmailHelper(string smtpHost, int smtpPort, string smtpUserName, string smtpPassword)
        {
            SMTPHost = smtpHost;
            SMTPPort = smtpPort;
            SMTPUserName = smtpUserName;
            SMTPPassword = smtpPassword;
        }

        public EmailHelper(string smtpHost, int smtpPort, string smtpUserName, string smtpPassword, List<Attachment> attachments)
        {
            SMTPHost = smtpHost;
            SMTPPort = smtpPort;
            SMTPUserName = smtpUserName;
            SMTPPassword = smtpPassword;
            Attachments = attachments;
        }

        public Boolean SendEmail(String from, DataTable recipients, String subject, String body, List<Attachment> attachmentList)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();

                mail.IsBodyHtml = true;
                mail.Subject = subject;
                mail.Body = body;
                MailAddress fromAdd = new MailAddress(from, from);
                mail.From = new MailAddress(fromAdd.ToString(), "KE Social Listening");

               // var tos = recipients.Split(";".ToCharArray());

                foreach (DataRow to in recipients.Rows)
                {
                    MailAddress toadd = new MailAddress(to["Email"].ToString());
                    mail.To.Add(toadd);
                }

                if (attachmentList != null)
                {
                    foreach (var atch in attachmentList)
                    {
                        mail.Attachments.Add(atch);
                    }
                }

                SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = SMTPHost,
                    Port = SMTPPort,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(SMTPUserName, SMTPPassword)
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public Boolean SendEmail(String from, DataTable recipients, String subject, String body)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();

                mail.IsBodyHtml = true;
                mail.Subject = subject;
                mail.Body = body;
                MailAddress fromAdd = new MailAddress(from, subject);
                mail.From = new MailAddress(fromAdd.ToString(), "KE Social Listening");
                
                foreach (DataRow to in recipients.Rows)
                {
                    MailAddress toadd = new MailAddress(to["Email"].ToString());
                    mail.To.Add(toadd);
                }

                SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = SMTPHost,
                    Port = SMTPPort,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(SMTPUserName, SMTPPassword)
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
