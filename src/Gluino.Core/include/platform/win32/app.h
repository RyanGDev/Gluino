#pragma once

#ifndef GLUINO_APP_H
#define GLUINO_APP_H

#include "app_base.h"
#include "window.h"
#include "webview.h"

#include <Windows.h>

#define WM_USER_INVOKE (WM_USER + 0x0002)

namespace Gluino {

class App final : public AppBase {
public:
    explicit App(HINSTANCE hInstance, const wchar_t* appId);
    ~App() override = default;

    void SpawnWindow(
        WindowOptions* windowOptions, WindowEvents* windowEvents,
        WebViewOptions* webViewOptions, WebViewEvents* webViewEvents,
        WindowBase** window, WebViewBase** webView) override;
    void DespawnWindow(WindowBase* window) override;
    void Run() override;
    void Exit() override;

    static HINSTANCE GetHInstance();
    static const wchar_t* GetWndClassName();
    static LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

private:
    HINSTANCE _hInstance;
    ptr<VisualStyleContext> _visualStyleContext;
    cppstr _appId;
    cppstr _wndClassName;
};

}

#endif // !GLUINO_APP_H