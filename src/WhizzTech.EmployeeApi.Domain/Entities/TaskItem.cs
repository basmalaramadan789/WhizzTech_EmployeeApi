namespace WhizzTech.EmployeeApi.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime DueDate { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string name, string description, DateTime dueDate)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            DueDate = dueDate
        };
    }
}
