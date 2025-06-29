Imports Microsoft.Web.WebView2.Wpf
Imports Microsoft.Web.WebView2.Core
Imports System.IO
Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadBookmarksFromFile()
        AddNewTab("https://www.bing.com") ' ✅ Start with Home tab through code
    End Sub
    Private Sub Go_Click(sender As Object, e As RoutedEventArgs)
        Dim tab = TryCast(BrowserTabs.SelectedItem, TabItem)
        If tab IsNot Nothing Then
            Dim web = TryCast(tab.Content, WebView2)
            If web IsNot Nothing Then
                Try
                    web.Source = New Uri(AddressBar.Text)
                Catch ex As Exception
                    MessageBox.Show("Invalid URL")
                End Try
            End If
        End If
    End Sub
    Private Sub Back_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing AndAlso web.CanGoBack Then
            web.GoBack()
        End If
    End Sub
    Private Sub Forward_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing AndAlso web.CanGoForward Then
            web.GoForward()
        End If
    End Sub
    Private Function GetCurrentWebView() As WebView2
        Dim tab = TryCast(BrowserTabs.SelectedItem, TabItem)
        Return TryCast(tab?.Content, WebView2)
    End Function
    Private Sub NewTab_Click(sender As Object, e As RoutedEventArgs)
        AddNewTab("https://www.bing.com")
    End Sub
    Private Async Sub AddNewTab(Optional url As String = "https://www.bing.com")
        ' Create tab header with close button
        Dim tabHeader As New StackPanel With {.Orientation = Orientation.Horizontal}
        Dim tabText As New TextBlock With {.Text = "New Tab", .Margin = New Thickness(0, 0, 5, 0)}
        Dim closeBtn As New Button With {.Content = "🗙", .Width = 20, .Height = 20, .Padding = New Thickness(0), .Margin = New Thickness(0)}
        closeBtn.Style = CType(FindResource(ToolBar.ButtonStyleKey), Style)

        Dim newTab As New TabItem()
        tabHeader.Tag = newTab
        newTab.Header = tabHeader

        AddHandler closeBtn.Click, Sub()
                                       BrowserTabs.Items.Remove(tabHeader.Tag)
                                   End Sub

        tabHeader.Children.Add(tabText)
        tabHeader.Children.Add(closeBtn)

        ' Create WebView2 instance
        Dim web As New WebView2 With {
        .Margin = New Thickness(0)
    }

        newTab.Content = web
        BrowserTabs.Items.Add(newTab)
        BrowserTabs.SelectedItem = newTab

        ' Ensure it's initialized BEFORE setting handlers
        Await web.EnsureCoreWebView2Async()

        ' Handle navigation completion (update address bar + title)
        AddHandler web.NavigationCompleted, Sub()
                                                tabText.Text = web.Source.Host
                                                AddressBar.Text = web.Source.ToString()
                                            End Sub

        ' Handle "target=_blank" or external window links
        AddHandler web.CoreWebView2.NewWindowRequested, Sub(sender, args)
                                                            args.Handled = True
                                                            web.CoreWebView2.Navigate(args.Uri)
                                                        End Sub

        ' Navigate to URL
        web.Source = New Uri(url)

        ' Track in history
        AttachHistoryTracking(web)
    End Sub


    Private Sub SaveBookmark_Click(sender As Object, e As RoutedEventArgs)
        Dim web = GetCurrentWebView()
        If web IsNot Nothing Then
            BookmarkList.Items.Add(web.Source.ToString())
        End If
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
    Private Sub RemoveBookmark_Click(sender As Object, e As RoutedEventArgs)
        If BookmarkList.SelectedItem IsNot Nothing Then
            BookmarkList.Items.Remove(BookmarkList.SelectedItem)
        End If
    End Sub

    Private Sub ClearHistory_Click(sender As Object, e As RoutedEventArgs)
        HistoryPanel.Items.Clear()
    End Sub

End Class