namespace NotificationService.CommandIdempotency
{
    public interface IIdempotentCommand
    {        
        string GetIdempotencyKey();
    }
}
