using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class FileAttachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ContentType { get; set; }

        public long SizeBytes { get; set; }

        [Required]
        public string StoredPath { get; set; }

        [Required]
        public string UploadedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}