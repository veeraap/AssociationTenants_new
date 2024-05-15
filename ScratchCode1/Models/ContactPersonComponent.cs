namespace ScratchCode;

/// Derived class implementing a people display component with a list of persons.
public class ContactPersonComponent : Component
{
    public override ComponentType Type => ComponentType.ContactPerson;

    /// List of persons displayed in the component.
    public List<Person> Persons { get; set; }

    /// Method to populate the component's properties.
    public override void PopulateProperties()
    {
        // Implementation to populate the properties
    }
}

public class Person
{
    public Guid Id { get; set; }
}