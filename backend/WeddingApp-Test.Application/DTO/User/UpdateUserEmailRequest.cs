using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.User;

public record UpdateUserEmailRequest([EmailAddress] string Email);
