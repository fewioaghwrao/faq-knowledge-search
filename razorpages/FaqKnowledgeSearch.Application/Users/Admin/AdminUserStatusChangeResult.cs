namespace FaqKnowledgeSearch.Application.Users.Admin;

public enum AdminUserStatusChangeResult
{
    Success,
    NotFound,
    CannotChangeAdmin,
    CannotChangeCurrentUser,
    Failed
}
