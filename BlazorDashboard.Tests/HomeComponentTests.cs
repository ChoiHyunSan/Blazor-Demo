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
        new Note(1, "첫 노트",  DateTime.UtcNow),
        new Note(2, "둘째 노트", DateTime.UtcNow),
    };

    // NoteService 생성자가 NpgsqlDataSource를 받는다면 base(null!) 호출 필요
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
        // 테스트 전용 DI 컨테이너: 실제 Program.cs와 무관
        // Home.razor에서 @inject NoteService Notes 를 Fake로 대체
        Services.AddSingleton<NoteService, FakeNoteService>();
    }

    [Fact]
    public void Home_Shows_SeedNotes()
    {
        // Arrange & Act
        var cut = RenderComponent<Home>();

        // Assert: 화면에 씨드 노트 2개가 보이는가?
        var html = cut.Markup;
        Assert.Contains("첫 노트", html);
        Assert.Contains("둘째 노트", html);
    }

    [Fact]
    public void Home_AddNote_Updates_List()
    {
        // Arrange
        var cut = RenderComponent<Home>();

        // 입력창에 값 입력하고 "추가" 버튼 클릭
        var input = cut.Find("input.form-control");
        input.Change("새로운 노트");

        var addButton = cut.Find("button.btn.btn-primary");
        addButton.Click();

        // Assert: 새 항목이 렌더링되었는지 확인
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("새로운 노트", cut.Markup);
        });
    }

    [Fact]
    public void Home_Delete_Removes_Item()
    {
        var cut = RenderComponent<Home>();

        // 삭제 대상 텍스트가 실제로 렌더되었는지 선확인
        Assert.Contains("첫 노트", cut.Markup);

        // li 중에서 "첫 노트" 텍스트를 포함하는 요소를 찾고
        var liForFirst = cut.FindAll("ul.list-group li")
                            .First(li => li.TextContent.Contains("첫 노트"));

        // (핵심 수정) IElement에는 Find가 없으므로 QuerySelector 사용
        var deleteBtn = liForFirst.QuerySelector("button.btn-outline-danger");
        Assert.NotNull(deleteBtn);

        deleteBtn!.Click();

        // 렌더 반영까지 기다리면서 "첫 노트"가 사라졌는지 확인
        cut.WaitForState(() => !cut.Markup.Contains("첫 노트"), TimeSpan.FromSeconds(3));
    }
}
