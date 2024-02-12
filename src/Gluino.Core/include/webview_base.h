#pragma once

#ifndef GLUINO_WEBVIEW_BASE_H
#define GLUINO_WEBVIEW_BASE_H

#include "webview_options.h"
#include "webview_events.h"
#include "window_base.h"

namespace Gluino {

class WebViewBase {
public:
    explicit WebViewBase(const WebViewOptions* options, const WebViewEvents* events) {
        _userDataPath = options->UserDataPath ? options->UserDataPath : L"";
        _startUrl = options->StartUrl ? options->StartUrl : L"";
        _startContent = options->StartContent ? options->StartContent : L"";
        _userAgent = options->UserAgent ? options->UserAgent : L"";

        _onCreated = (Delegate)events->OnCreated;
        _onNavigationStart = (StringDelegate)events->OnNavigationStart;
        _onNavigationEnd = (Delegate)events->OnNavigationEnd;
        _onMessageReceived = (StringDelegate)events->OnMessageReceived;
        _onResourceRequested = (WebResourceDelegate)events->OnResourceRequested;
    }
    virtual ~WebViewBase() = default;

    virtual void Attach(WindowBase* window) = 0;
    virtual void Navigate(cstr url) = 0;
    virtual void NativateToString(cstr content) = 0;
    virtual void PostWebMessage(cstr message) = 0;
    virtual void InjectScript(cstr script, bool onDocumentCreated) = 0;

    virtual ccstr GetUserDataPath() = 0;

    virtual bool GetContextMenuEnabled() = 0;
    virtual void SetContextMenuEnabled(bool enabled) = 0;

    virtual bool GetDevToolsEnabled() = 0;
    virtual void SetDevToolsEnabled(bool enabled) = 0;

    virtual ccstr GetUserAgent() = 0;
    virtual void SetUserAgent(cstr userAgent) = 0;

protected:
    cppstr _userDataPath;
    cppstr _startUrl;
    cppstr _startContent;
    cppstr _userAgent;

    Delegate _onCreated;
    StringDelegate _onNavigationStart;
    Delegate _onNavigationEnd;
    StringDelegate _onMessageReceived;
    WebResourceDelegate _onResourceRequested;
};

} // namespace Gluino

#endif // !GLUINO_WEBVIEW_BASE_H
