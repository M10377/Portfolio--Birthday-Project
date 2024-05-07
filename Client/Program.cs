using System;
using System.Linq;
using System.Threading;
using Spectre.Console;

public class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            Breadcrumb.Draw();

            var (familyAction, family) = FamilyUI.SelectOne();

            if (familyAction == FamilyUI.SelectOption.ShowReport)
            {
                ShowAllFamilyReport();
                continue;
            }
            else if (familyAction == FamilyUI.SelectOption.AssignSecretSanta)
            {
                Console.WriteLine("Running Secret Santa...");
                AssignSecretSanta();
                Console.WriteLine("Secret Santa completed.");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else if (familyAction == FamilyUI.SelectOption.InsertSampleFamilies)
            {
                InsertSampleFamilies(true);
                continue;
            }
            else if (familyAction == FamilyUI.SelectOption.CreateNewFamily)
            {
                family = FamilyUI.Create();
            }

            PresentFamily(family);
        }
    }

    static void AssignSecretSanta()
    {
        var families = Database.ListExistingFamilies().Select(x => new Database(x.Name));

        foreach (var family in families)
        {
            foreach (var member in family.Family!.Members)
            {
                if (TryFetchRandomMember(member.GetUniqueName(family.Family.Name), families, out var assignee))
                {
                    member.GiveToName = assignee!.Value.Name;
                    member.GiveToGiftIdea = assignee!.Value.GiftIdea;
                    family.Save();
                    Console.WriteLine($"{family.Family.Name}/{member.Name} assigned to {member.GiveToName}.");
                }
            }
        }
    }

    static bool TryFetchRandomMember(string uniqueName, IEnumerable<Database> families, out (string Name, string GiftIdea)? assignee)
    {
        assignee = families
            .SelectMany(x => x.Family!.Members.Select(y => new { UniqueName = y.GetUniqueName(x.Family.Name), Member = y }))
            .Where(x => x.UniqueName != uniqueName)
            .Where(x => string.IsNullOrEmpty(x.Member.GiveToName) || x.Member.GiveToName != "-")
            .Where(x => !x.Member.AvoidMembers.Contains(uniqueName))
            .OrderBy(x => Guid.NewGuid())
            .Select(x => (x.Member.Name, x.Member.GiftIdea))
            .FirstOrDefault();

        return assignee is not null;
    }

    static void ShowAllFamilyReport()
{
    var allFamilies = Database.ListExistingFamilies();

    if (allFamilies.Count() == 0)
    {
        Console.WriteLine("No families found.");
        return;
    }

    foreach (var family in allFamilies)
    {
        Console.WriteLine($"Family: {family.Name}");
        Console.WriteLine("Members:");
        var path = Database.NameToPath(family.FileName);
        var json = System.IO.File.ReadAllText(path);
        var familyObj = System.Text.Json.JsonSerializer.Deserialize<Family>(json);
        if (familyObj != null)
        {
            foreach (var member in familyObj.Members)
            {
                Console.WriteLine($"- {member.Name}: Gift to {member.GiveToName} (Gift Idea: {member.GiveToGiftIdea})");
            }
        }
        else
        {
            Console.WriteLine("Error: Failed to deserialize family data.");
        }
        Console.WriteLine();
    }

    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}



    static void InsertSampleFamilies(bool clearFirst = false)
    {
        if (clearFirst)
        {
            // Clear existing families
            Database.ClearAll();
        }

        // Sample family 1
        var smithFamily = new Database("Smith");
        smithFamily.Family!.Members.Add(new Member { Name = "John", Birthday = new DateOnly(1980, 1, 1), GiftIdea = "Books" });
        smithFamily.Family!.Members.Add(new Member { Name = "Jane", Birthday = new DateOnly(1982, 2, 2), GiftIdea = "Cookware" });
        smithFamily.Save();

        // Sample family 2
        var doeFamily = new Database("Doe");
        doeFamily.Family!.Members.Add(new Member { Name = "Alice", Birthday = new DateOnly(1975, 3, 3), GiftIdea = "Music CDs" });
        doeFamily.Family!.Members.Add(new Member { Name = "Bob", Birthday = new DateOnly(1978, 4, 4), GiftIdea = "Tech gadgets" });
        doeFamily.Save();

        Console.WriteLine("Sample families inserted.");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    static void PresentFamily(Database? family)
    {
        if (family == null)
        {
            Console.WriteLine("No family selected.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        Breadcrumb.Forward(family.Family!.Name);

        while (true)
        {
            Breadcrumb.Draw(true);
            FamilyUI.Show(family);
            var familyOption = FamilyUI.Menu(family);

            if (familyOption == FamilyUI.MenuOption.Rename)
            {
                FamilyUI.Edit(family);
                family.Save();
                continue;
            }
            else if (familyOption == FamilyUI.MenuOption.OpenMember)
            {
                var member = MemberUI.SelectOne(family);
                PresentMember(family, member);
                continue;
            }
            else if (familyOption == FamilyUI.MenuOption.AddMember)
            {
                var member = MemberUI.Create(family);
                family.Family!.Members.Add(member);
                family.Save();
                continue;
            }
            else if (familyOption == FamilyUI.MenuOption.Delete)
            {
                FamilyUI.Delete(family);
            }

            Breadcrumb.Back();
            return;
        }
    }

    static void PresentMember(Database family, Member member)
    {
        Breadcrumb.Forward(member.Name);

        while (true)
        {
            Breadcrumb.Draw(true);
            MemberUI.Show(member);

            var selection = MemberUI.Menu(member);
            if (selection == MemberUI.MenuOption.Edit)
            {
                MemberUI.Edit(family, member);
                family.Save();
                continue;
            }
            else if (selection == MemberUI.MenuOption.Delete)
            {
                MemberUI.Delete(family, member);
                family.Save();
            }

            Breadcrumb.Back();
            return;
        }
    }
}
