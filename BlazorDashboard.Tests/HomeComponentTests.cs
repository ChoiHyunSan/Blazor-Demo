using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using BlazorDashboard.Components.Pages; 
using BlazorDashboard.Services;

public sealed class FakeNoteService : NoteService
{
    private readonly List<Note> _store = new()
    {
        new Note(1, "ù ��Ʈ",  DateTime.UtcNow),
        new Note(2, "��° ��Ʈ", DateTime.UtcNow),
    };

    // NoteService �����ڰ� NpgsqlDataSource�� �޴´ٸ� base(null!) ȣ�� �ʿ�
    public FakeNoteService() : base(null!) { }

    public override Task<IEnumerable<Note>> GetNotesAsync()
        => Task.FromResult<IEnumerable<Note>>(_store.OrderByDescending(x => x.Id).ToList());

    public override Task<long> AddNoteAsync(string title)
    {
        var nextId = _store.Count == 0 ? 1 : _store.Max(x => x.Id) + 1;
        _store.Add(new Note(nextId, title, DateTime.UtcNow));
        return Task.FromResult((long)nextId);
    }

    public override Task<int> DeleteNoteAsync(long id)
    {
        var removed = _store.RemoveAll(x => x.Id == id);
        return Task.FromResult(removed);
    }
}

public class HomeComponentTests : TestContext
{
    public HomeComponentTests()
    {
        // �׽�Ʈ ���� DI �����̳�: ���� Program.cs�� ����
        // Home.razor���� @inject NoteService Notes �� Fake�� ��ü
        Services.AddSingleton<NoteService, FakeNoteService>();
    }

    [Fact]
    public void Home_Shows_SeedNotes()
    {
        // Arrange & Act
        var cut = RenderComponent<Home>();

        // Assert: ȭ�鿡 ���� ��Ʈ 2���� ���̴°�?
        var html = cut.Markup;
        Assert.Contains("ù ��Ʈ", html);
        Assert.Contains("��° ��Ʈ", html);
    }

    [Fact]
    public void Home_AddNote_Updates_List()
    {
        // Arrange
        var cut = RenderComponent<Home>();

        // �Է�â�� �� �Է��ϰ� "�߰�" ��ư Ŭ��
        var input = cut.Find("input.form-control");
        input.Change("���ο� ��Ʈ");

        var addButton = cut.Find("button.btn.btn-primary");
        addButton.Click();

        // Assert: �� �׸��� �������Ǿ����� Ȯ��
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("���ο� ��Ʈ", cut.Markup);
        });
    }

    [Fact]
    public void Home_Delete_Removes_Item()
    {
        var cut = RenderComponent<Home>();

        // ���� ��� �ؽ�Ʈ�� ������ �����Ǿ����� ��Ȯ��
        Assert.Contains("ù ��Ʈ", cut.Markup);

        // li �߿��� "ù ��Ʈ" �ؽ�Ʈ�� �����ϴ� ��Ҹ� ã��
        var liForFirst = cut.FindAll("ul.list-group li")
                            .First(li => li.TextContent.Contains("ù ��Ʈ"));

        // (�ٽ� ����) IElement���� Find�� �����Ƿ� QuerySelector ���
        var deleteBtn = liForFirst.QuerySelector("button.btn-outline-danger");
        Assert.NotNull(deleteBtn);

        deleteBtn!.Click();

        // ���� �ݿ����� ��ٸ��鼭 "ù ��Ʈ"�� ��������� Ȯ��
        cut.WaitForState(() => !cut.Markup.Contains("ù ��Ʈ"), TimeSpan.FromSeconds(3));
    }
}
