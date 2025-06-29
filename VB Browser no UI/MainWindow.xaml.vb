Imports Microsoft.Web.WebView2.Wpf
Imports Microsoft.Web.WebView2.Core
Imports System.IO

Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ThemeToggle.IsChecked = False ' Light theme by default
        LoadBookmarksFromFile()
        AddNewTab("https://www.bing.com")
    End Sub

    Private Sub AddNewTab(url As String)
        Dim tab As New TabItem()

        ' Create a StackPanel for the header with URL and close button
        Dim headerPanel As New StackPanel()
        headerPanel.Orientation = Orientation.Horizontal

        ' Add URL text
        Dim urlText As New TextBlock()
        urlText.Text = url
        urlText.Margin = New Thickness(0, 0, 5, 0)
        headerPanel.Children.Add(urlText)

        ' Add close button
        Dim closeButton As New Button()
        closeButton.Content = "×"
        closeButton.Width = 20
        closeButton.Height = 20
        closeButton.FontSize = 14
        closeButton.FontWeight = FontWeights.Bold
        closeButton.Background = Brushes.Transparent
        closeButton.BorderThickness = New Thickness(0)
        closeButton.Margin = New Thickness(5, 0, 0, 0)
        closeButton.ToolTip = "Close Tab"

        ' Handle close button click
        AddHandler closeButton.Click, Sub(sender, e)
                                          CloseTab(tab)
                                      End Sub

        headerPanel.Children.Add(closeButton)
        tab.Header = headerPanel

        ' Create and setup WebView2
        Dim browser As New WebView2()
        browser.Source = New Uri(url)
        browser.EnsureCoreWebView2Async()
        tab.Content = browser

        ' Add tab to TabControl
        BrowserTabs.Items.Add(tab)
        BrowserTabs.SelectedItem = tab
        AttachHistoryTracking(browser)
    End Sub

    Private Sub CloseTab(tabToClose As TabItem)
        ' Don't close if it's the last tab
        If BrowserTabs.Items.Count <= 1 Then
            Return
        End If

        ' Get the WebView2 control and dispose it properly
        Dim browser As WebView2 = TryCast(tabToClose.Content, WebView2)
        If browser IsNot Nothing Then
            browser.Dispose()
        End If

        ' Remove the tab
        BrowserTabs.Items.Remove(tabToClose)

        ' Select another tab if the closed tab was selected
        If BrowserTabs.Items.Count > 0 AndAlso BrowserTabs.SelectedItem Is Nothing Then
            BrowserTabs.SelectedIndex = 0
        End If
    End Sub

    ' Required method for XAML binding - add this to your MainWindow.xaml.vb file:
    Private Sub CloseTab_Click(sender As Object, e As RoutedEventArgs)
        ' Get the button that was clicked
        Dim closeButton As Button = TryCast(sender, Button)
        If closeButton IsNot Nothing Then
            ' Find the parent TabItem by traversing up the visual tree
            Dim parent As DependencyObject = closeButton
            While parent IsNot Nothing AndAlso Not TypeOf parent Is TabItem
                parent = VisualTreeHelper.GetParent(parent)
            End While

            If parent IsNot Nothing Then
                Dim tab As TabItem = TryCast(parent, TabItem)
                CloseTab(tab)
            End If
        End If
    End Sub

    ' Alternative method if you want to close tab by right-click context menu
    Private Sub SetupTabContextMenu(tab As TabItem)
        Dim contextMenu As New ContextMenu()

        Dim closeMenuItem As New MenuItem()
        closeMenuItem.Header = "Close Tab"
        AddHandler closeMenuItem.Click, Sub(sender, e)
                                            CloseTab(tab)
                                        End Sub

        contextMenu.Items.Add(closeMenuItem)
        tab.ContextMenu = contextMenu
    End Sub
    Private Sub Go_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing Then
            Try
                web.Source = New Uri(AddressBar.Text)
            Catch ex As Exception
                MessageBox.Show("Invalid URL")
            End Try
        End If
    End Sub

    Private Sub Back_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing AndAlso web.CanGoBack Then web.GoBack()
    End Sub

    Private Sub Forward_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing AndAlso web.CanGoForward Then web.GoForward()
    End Sub

    Private Function GetCurrentWebView() As WebView2
        Dim tab = TryCast(BrowserTabs.SelectedItem, TabItem)
        Return TryCast(tab?.Content, WebView2)
    End Function

    Private Sub NewTab_Click(sender As Object, e As RoutedEventArgs)
        AddNewTab("https://www.bing.com")
    End Sub

    Private Sub SaveBookmark_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing Then BookmarkList.Items.Add(web.Source.ToString())
    End Sub

    Private Sub RemoveBookmark_Click(sender As Object, e As RoutedEventArgs)
        If BookmarkList.SelectedItem IsNot Nothing Then BookmarkList.Items.Remove(BookmarkList.SelectedItem)
    End Sub

    Private Sub SaveBookmarksToFile_Click(sender As Object, e As RoutedEventArgs)
        File.WriteAllLines("bookmarks.txt", BookmarkList.Items.Cast(Of String)())
    End Sub

    Private Sub LoadBookmarksFromFile()
        If File.Exists("bookmarks.txt") Then
            BookmarkList.Items.Clear()
            For Each line In File.ReadAllLines("bookmarks.txt")
                BookmarkList.Items.Add(line)
            Next
        End If
    End Sub

    Private Sub ClearHistory_Click(sender As Object, e As RoutedEventArgs)
        HistoryPanel.Items.Clear()
    End Sub

    Private Sub AttachHistoryTracking(web As WebView2)
        AddHandler web.NavigationCompleted, Sub()
                                                If web.Source IsNot Nothing Then
                                                    HistoryPanel.Items.Add(DateTime.Now.ToString("HH:mm:ss") & " - " & web.Source.ToString())
                                                End If
                                            End Sub
    End Sub

    Private Sub ApplyTheme(path As String)
        Dim dict = New ResourceDictionary()
        dict.Source = New Uri(path, UriKind.Relative)
        Application.Current.Resources.MergedDictionaries.Clear()
        Application.Current.Resources.MergedDictionaries.Add(dict)
    End Sub

    Private Sub ThemeToggle_Checked(sender As Object, e As RoutedEventArgs)
        ThemeToggle.Content = "☀"
        ApplyTheme("Themes/Dark.xaml")
    End Sub

    Private Sub ThemeToggle_Unchecked(sender As Object, e As RoutedEventArgs)
        ThemeToggle.Content = "🌙"
        ApplyTheme("Themes/Light.xaml")
    End Sub

End Class
