using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rectangle.Windows.WinUI.Views;

public sealed class ActionSearchWindow : Window
{
    private readonly WindowManager _windowManager;
    private readonly AutoSuggestBox _searchBox;
    private readonly ListView _listView;
    private readonly List<ActionItem> _allItems;

    public ActionSearchWindow(WindowManager windowManager)
    {
        _windowManager = windowManager;
        Title = "搜索动作";
        _allItems = BuildActionItems();

        _searchBox = new AutoSuggestBox
        {
            PlaceholderText = "输入动作名或标签，例如：左半屏、Undo、Third",
            Margin = new Thickness(12, 12, 12, 8)
        };
        _searchBox.TextChanged += (_, e) =>
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange) return;
            RefreshList(_searchBox.Text);
        };
        _searchBox.QuerySubmitted += (_, e) =>
        {
            if (e.ChosenSuggestion is ActionItem chosen) Execute(chosen);
            else if (_listView.Items.FirstOrDefault() is ActionItem first) Execute(first);
        };

        _listView = new ListView
        {
            Margin = new Thickness(12, 0, 12, 12),
            IsItemClickEnabled = true,
            SelectionMode = ListViewSelectionMode.Single
        };
        _listView.ItemClick += (_, e) =>
        {
            if (e.ClickedItem is ActionItem item) Execute(item);
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            Children = { _searchBox, _listView }
        };
        Grid.SetRow(_listView, 1);

        RefreshList(string.Empty);
    }

    private void RefreshList(string keyword)
    {
        var q = (keyword ?? string.Empty).Trim();
        var filtered = string.IsNullOrEmpty(q)
            ? _allItems
            : _allItems.Where(i =>
                i.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                i.Tag.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _searchBox.ItemsSource = filtered;
        _listView.ItemsSource = filtered;
    }

    private void Execute(ActionItem item)
    {
        _windowManager.Execute(item.Action, forceDirectAction: true);
        Close();
    }

    private List<ActionItem> BuildActionItems()
    {
        var list = new List<ActionItem>();
        foreach (var action in Enum.GetValues<WindowAction>())
        {
            list.Add(new ActionItem
            {
                Action = action,
                Tag = action.ToString(),
                DisplayName = action.ToString()
            });
        }
        return list;
    }

    private sealed class ActionItem
    {
        public WindowAction Action { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public override string ToString() => $"{DisplayName} ({Tag})";
    }
}
