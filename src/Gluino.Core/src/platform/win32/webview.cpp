#include "webview.h"

#include <iostream>
#include <shlobj.h>
#include <Shlwapi.h>
#include <wrl.h>

using namespace Microsoft::WRL;
using namespace Gluino;

WebView::~WebView() {
    _webviewSettings2.reset();
    _webviewSettings.reset();
    _webviewController2.reset();
    _webviewController.reset();
    _webviewEnv.reset();
    _webview.reset();
}

void WebView::Refit(const WindowBorderStyle& borderStyle) const {
    if (_webviewController == nullptr) return;

    RECT bounds;
    GetClientRect(_hWndWnd, &bounds);
    if (borderStyle == WindowBorderStyle::SizableNoCaption ||
        borderStyle == WindowBorderStyle::FixedNoCaption) {
        bounds.top += 1;
        bounds.left += 1;
        bounds.right -= 1;
        bounds.bottom -= 1;
    }

    _webviewController->put_Bounds(bounds);
}

void WebView::Focus() const {
    if (!_webviewController) return;
    _webviewController->MoveFocus(COREWEBVIEW2_MOVE_FOCUS_REASON_PROGRAMMATIC);
}

void WebView::NotifyParentWindowPositionChanged() const {
    if (!_webviewController) return;
    _webviewController->NotifyParentWindowPositionChanged();
}

void WebView::Attach(WindowBase* window) {
    _window = (Window*)window;
    _hWndWnd = _window->GetHandle();

    auto dataPath = _userDataPath;
    if (dataPath.empty()) {
        if (wchar_t specialFolderPath[MAX_PATH];
            SHGetSpecialFolderPath(nullptr, specialFolderPath, CSIDL_LOCAL_APPDATA, FALSE)) {
            dataPath = specialFolderPath;
            dataPath += L"\\Gluino";
        }
    }

    CreateCoreWebView2EnvironmentWithOptions(nullptr, dataPath.c_str(), nullptr,
        Callback<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>(this,
            &WebView::OnWebView2CreateEnvironmentCompleted).Get());
}

void WebView::Navigate(const cstr url) {
    if (_webview == nullptr) return;
    _webview->Navigate(url);
}

void WebView::NativateToString(const cstr content) {
    if (_webview == nullptr) return;
    _webview->NavigateToString(content);
}

void WebView::PostWebMessage(const cstr message) {
    if (_webview == nullptr) return;
    _webview->PostWebMessageAsString(message);
}

void WebView::InjectScript(cstr script, bool onDocumentCreated) {
    if (_webview == nullptr) return;
    if (onDocumentCreated)
        _webview->AddScriptToExecuteOnDocumentCreated(script, nullptr);
    else
        _webview->ExecuteScript(script, nullptr);
}

void WebView::ClearAutoFill() {
    if (_webview15 == nullptr) return;

    wil::com_ptr<ICoreWebView2Profile> profile;
    _webview15->get_Profile(&profile);

    const auto profile2 = profile.query<ICoreWebView2Profile2>();
    if (profile2 == nullptr) return;

    constexpr auto dataKinds =
        COREWEBVIEW2_BROWSING_DATA_KINDS_GENERAL_AUTOFILL |
        COREWEBVIEW2_BROWSING_DATA_KINDS_PASSWORD_AUTOSAVE;

    profile2->ClearBrowsingData(dataKinds, Callback<ICoreWebView2ClearBrowsingDataCompletedHandler>(
        [this](HRESULT) -> HRESULT {
        return S_OK;
    }).Get());
}

ccstr WebView::GetUserDataPath() {
    return _userDataPath.c_str();
}

bool WebView::GetGrantPermissions() const {
    return _grantPermissions;
}

bool WebView::GetContextMenuEnabled() {
    BOOL enabled;
    _webviewSettings->get_AreDefaultContextMenusEnabled(&enabled);
    return enabled;
}

void WebView::SetContextMenuEnabled(const bool enabled) {
    _webviewSettings->put_AreDefaultContextMenusEnabled(enabled);
}

bool WebView::GetDevToolsEnabled() {
    BOOL enabled;
    _webviewSettings->get_AreDevToolsEnabled(&enabled);
    return enabled;
}

void WebView::SetDevToolsEnabled(bool enabled) {
    _webviewSettings->put_AreDevToolsEnabled(enabled);
}

ccstr WebView::GetUserAgent() {
    wchar_t* userAgent;
    _webviewSettings2->get_UserAgent(&userAgent);
    return userAgent;
}

void WebView::SetUserAgent(const cstr userAgent) {
    _webviewSettings2->put_UserAgent(userAgent);
}

HRESULT WebView::OnWebView2CreateEnvironmentCompleted(const HRESULT result, ICoreWebView2Environment* env) {
    if (result != S_OK) return result;
    const auto envResult = env->QueryInterface(&_webviewEnv);
    if (envResult != S_OK) return envResult;

    env->CreateCoreWebView2Controller(_hWndWnd,
        Callback<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler>(this,
            &WebView::OnWebView2CreateControllerCompleted).Get());

    return S_OK;
}

HRESULT WebView::OnWebView2CreateControllerCompleted(const HRESULT result, ICoreWebView2Controller* controller) {
    if (result != S_OK) return result;

    const auto controllerResult = controller->QueryInterface(&_webviewController);
    if (controllerResult != S_OK) return controllerResult;

    _webviewController->get_CoreWebView2(&_webview);

    _webviewController2 = _webviewController.query<ICoreWebView2Controller2>();
    _webviewController2->put_DefaultBackgroundColor({ 0, 0, 0, 0 });

    _webview11 = _webview.query<ICoreWebView2_11>();
    _webview15 = _webview.query<ICoreWebView2_15>();

    _webview->get_Settings(&_webviewSettings);
    _webviewSettings->put_AreHostObjectsAllowed(TRUE);
    _webviewSettings->put_IsScriptEnabled(TRUE);
    _webviewSettings->put_AreDefaultScriptDialogsEnabled(TRUE);
    _webviewSettings->put_IsWebMessageEnabled(TRUE);
    _webviewSettings->put_IsStatusBarEnabled(FALSE);
    _webviewSettings->put_AreDefaultContextMenusEnabled(_contextMenuEnabled);
    _webviewSettings->put_AreDevToolsEnabled(_devToolsEnabled);

    _webviewSettings2 = _webviewSettings.try_query<ICoreWebView2Settings2>();
    if (!_userAgent.empty()) _webviewSettings2->put_UserAgent(_userAgent.c_str());

    _webview->AddScriptToExecuteOnDocumentCreated(
        LR"(window.gluino = (function() {
  const listenerMap = new Map();

  return {
    sendMessage: function(message) {
      window.chrome.webview.postMessage(message);
    },
    addListener: function(callback) {
      let wrappedCallback = function(e) {
          const msg = e.data;
          callback(msg);
      };
      listenerMap.set(callback, wrappedCallback);
      window.chrome.webview.addEventListener('message', wrappedCallback);
    },
    removeListener: function(callback) {
      let wrappedCallback = listenerMap.get(callback);
      if (wrappedCallback) {
          window.chrome.webview.removeEventListener('message', wrappedCallback);
          listenerMap.delete(callback);
      }
    }
  };
})();)", nullptr);

    _onCreated();

    EventRegistrationToken navigationStartingToken;
    _webview->add_NavigationStarting(
        Callback<ICoreWebView2NavigationStartingEventHandler>(this,
            &WebView::OnWebView2NavigationStarting).Get(), &navigationStartingToken);

    EventRegistrationToken navigationCompletedToken;
    _webview->add_NavigationCompleted(
        Callback<ICoreWebView2NavigationCompletedEventHandler>(this,
            &WebView::OnWebView2NavigationCompleted).Get(), &navigationCompletedToken);

    EventRegistrationToken webMessageReceivedToken;
    _webview->add_WebMessageReceived(
        Callback<ICoreWebView2WebMessageReceivedEventHandler>(this,
            &WebView::OnWebView2WebMessageReceived).Get(), &webMessageReceivedToken);

    EventRegistrationToken webResourceRequestedToken;
    _webview->AddWebResourceRequestedFilter(L"*", COREWEBVIEW2_WEB_RESOURCE_CONTEXT_ALL);
    _webview->add_WebResourceRequested(
        Callback<ICoreWebView2WebResourceRequestedEventHandler>(this,
            &WebView::OnWebView2WebResourceRequested).Get(), &webResourceRequestedToken);

    EventRegistrationToken permissionRequestedToken;
    _webview->add_PermissionRequested(
        Callback<ICoreWebView2PermissionRequestedEventHandler>(this,
            &WebView::OnWebView2PermissionRequested).Get(), &permissionRequestedToken);

    EventRegistrationToken contextMenuRequestedToken;
    _webview11->add_ContextMenuRequested(
        Callback<ICoreWebView2ContextMenuRequestedEventHandler>(
            &WebView::OnWebView2ContextMenuRequested).Get(), &contextMenuRequestedToken);

    if (!_startUrl.empty()) 
        _webview->Navigate(_startUrl.c_str());
    else if (!_startContent.empty()) 
        _webview->NavigateToString(_startContent.c_str());

    Refit(_window->GetBorderStyle());

    return S_OK;
}

HRESULT WebView::OnWebView2NavigationStarting(ICoreWebView2* sender, ICoreWebView2NavigationStartingEventArgs* args) {
    wil::unique_cotaskmem_string uri;
    if (const auto hr = args->get_Uri(&uri); hr != S_OK)
        return hr;
    _onNavigationStart(uri.get());
    return S_OK;
}

HRESULT WebView::OnWebView2NavigationCompleted(ICoreWebView2* sender, ICoreWebView2NavigationCompletedEventArgs* args) {
    _onNavigationEnd();
    return S_OK;
}

HRESULT WebView::OnWebView2WebMessageReceived(ICoreWebView2* sender, ICoreWebView2WebMessageReceivedEventArgs* args) {
    wil::unique_cotaskmem_string message;
    if (const auto hr = args->TryGetWebMessageAsString(&message); hr != S_OK)
        return hr;
    _onMessageReceived(message.get());
    return S_OK;
}

HRESULT WebView::OnWebView2WebResourceRequested(ICoreWebView2* sender, ICoreWebView2WebResourceRequestedEventArgs* args) {
    ICoreWebView2WebResourceRequest* request;
    args->get_Request(&request);

    wil::unique_cotaskmem_string reqUri;
    request->get_Uri(&reqUri);

    wil::unique_cotaskmem_string reqMethod;
    request->get_Method(&reqMethod);

    const WebResourceRequest req{
        reqUri.get(),
        reqMethod.get(),
    };
    WebResourceResponse res;
    _onResourceRequested(req, &res);

    const wil::unique_cotaskmem content(res.Content);

    if (content != nullptr) {
        const auto reasonPhrase = cppstr(res.ReasonPhrase);
        const auto contentType = cppstr(res.ContentType);

        IStream* stream = SHCreateMemStream((BYTE*)content.get(), res.ContentLength);
        wil::com_ptr<ICoreWebView2WebResourceResponse> response;

        _webviewEnv->CreateWebResourceResponse(
            stream,
            res.StatusCode,
            reasonPhrase.c_str(),
            (L"" + contentType).c_str(),
            &response);

        args->put_Response(response.get());

        stream->Release();
    }

    return S_OK;
}

HRESULT WebView::OnWebView2PermissionRequested(ICoreWebView2* sender, ICoreWebView2PermissionRequestedEventArgs* args) {
    if (_grantPermissions)
        args->put_State(COREWEBVIEW2_PERMISSION_STATE_ALLOW);
    return S_OK;
}

HRESULT WebView::OnWebView2ContextMenuRequested(ICoreWebView2* sender, ICoreWebView2ContextMenuRequestedEventArgs* args) {
    const wil::com_ptr eventArgs = args;
    wil::com_ptr<ICoreWebView2ContextMenuItemCollection> menuItems;
    eventArgs->get_MenuItems(&menuItems);

    UINT32 menuItemsCount;
    menuItems->get_Count(&menuItemsCount);

    wil::com_ptr<ICoreWebView2ContextMenuItem> menuItem;
    for (UINT32 i = 0; i < menuItemsCount; i++) {
        menuItems->GetValueAtIndex(i, &menuItem);

        wil::unique_cotaskmem_string name;
        menuItem->get_Name(&name);

        if (wcscmp(name.get(), L"back") == 0 ||
            wcscmp(name.get(), L"forward") == 0 ||
            wcscmp(name.get(), L"reload") == 0 ||
            wcscmp(name.get(), L"other") == 0 ||
            wcscmp(name.get(), L"saveAs") == 0 ||
            wcscmp(name.get(), L"print") == 0 ||
            wcscmp(name.get(), L"share") == 0 ||
            wcscmp(name.get(), L"webCapture") == 0 ||
            wcscmp(name.get(), L"emoji") == 0 ||
            wcscmp(name.get(), L"copyLinkToHighlight") == 0) {
            menuItems->RemoveValueAtIndex(i);
            --i;
            --menuItemsCount;
        }
    }

    return S_OK;
}
