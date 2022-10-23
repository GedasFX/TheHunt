using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("submissions")]
public class Submission
{
    [Key, Column("id")]
    public ulong Id { get; set; }

    [MaxLength(400)]
    [Column("proof_url")]
    public string? ProofUrl { get; set; }

    [Column("status")]
    [DefaultValue("0")]
    public SubmissionStatus Status { get; set; }

    [Column("submission_date")]
    [DefaultValue("current_timestamp at time zone 'utc'")]
    public DateTime SubmissionDate { get; set; }
    
    [Column("verification_date")]
    public DateTime? VerificationDate { get; set; }
    
    [Column("submitter_id")]
    [ForeignKey(nameof(Submitter))]
    public ulong SubmitterId { get; set; }
    
    [Column("verifier_id")]
    [ForeignKey(nameof(Verifier))]
    public ulong? VerifierId { get; set; }

    public User? Submitter { get; set; }
    public User? Verifier { get; set; }
}

public enum SubmissionStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
}