#include "window_frame.h"
#include "app.h"

#include <unordered_map>

using namespace Gluino;

struct WindowInfo {
    WindowEdge edge;
    HWND hWndWindow;
    WindowFrame* frame;
};

WNDPROC OriginalWndProc = nullptr;
std::unordered_map<HWND, WindowInfo> StaticHandleToInfo;

WindowFrame::WindowFrame(const HWND hWndWindow) {
    _hWndWindow = hWndWindow;
}

WindowFrame::~WindowFrame() {
    Detach();
}

void WindowFrame::Attach() {
    if (_isAttached) return;
    for (int i = 0; i < 8; ++i) {
        const auto edge = static_cast<WindowEdge>(i);
        const auto [x, y, width, height] = GetEdgeRect(edge);
        const auto hWnd = CreateWindowEx(
            WS_EX_TRANSPARENT,
            L"STATIC",
            nullptr,
            WS_VISIBLE | WS_CHILD | SS_NOTIFY,
            x,
            y,
            width,
            height,
            _hWndWindow,
            nullptr,
            App::GetHInstance(),
            nullptr);

        _hWndEdges.push_back(hWnd);

        const WindowInfo info = {edge, _hWndWindow, this};
        StaticHandleToInfo[hWnd] = info;
        OriginalWndProc = (WNDPROC)SetWindowLongPtr(hWnd, GWLP_WNDPROC, (LONG_PTR)WndFrameProc);
    }
    _isAttached = true;
}

void WindowFrame::Detach() {
    if (!_isAttached) return;
    for (const auto hWnd : _hWndEdges) {
        StaticHandleToInfo.erase(hWnd);

        DestroyWindow(hWnd);
    }
    _isAttached = false;
}

void WindowFrame::Update() const {
    if (!_isAttached) return;
    for (const auto& [hWnd, info] : StaticHandleToInfo) {
        auto [x, y, width, height] = GetEdgeRect(info.edge);
        SetWindowPos(hWnd, nullptr, x, y, width, height, SWP_NOZORDER);
    }
}

Rect WindowFrame::GetEdgeRect(const WindowEdge edge) const {
    RECT wndRect;
    GetWindowRect(_hWndWindow, &wndRect);

    const auto width = wndRect.right - wndRect.left;
    const auto height = wndRect.bottom - wndRect.top;
    constexpr auto size = 8;
    constexpr auto hoffset = size*2;
    constexpr auto voffset = size*5;

    switch (edge) {
        case WindowEdge::Top:
            return { size, 0, width - size * 4 + hoffset, size };
        case WindowEdge::Bottom:
            return { size, height - size * 6 + voffset, width - size * 4 + hoffset, size };
        case WindowEdge::Left:
            return { 0, size, size, height - size * 7 + voffset };
        case WindowEdge::Right:
            return { width - size * 3 + hoffset, size, size, height - size * 7 + voffset };
        case WindowEdge::TopLeft:
            return { 0, 0, size, size };
        case WindowEdge::TopRight:
            return { width - size * 3 + hoffset, 0, size, size };
        case WindowEdge::BottomLeft:
            return { 0, height - size * 6 + voffset, size, size };
        case WindowEdge::BottomRight:
            return { width - size * 3 + hoffset, height - size * 6 + voffset, size, size };
    }

    return { 0, 0, 0, 0 };
}

LRESULT WindowFrame::WndFrameEdgeProc(HWND hWnd, WindowEdge edge, UINT msg, WPARAM wParam, LPARAM lParam) const {
    static std::unordered_map<WindowEdge, HCURSOR> edgeToCursor;

    switch (msg) {
        case WM_LBUTTONDOWN: {
            int ht = HTCLIENT;
            switch (edge) {
                case WindowEdge::Top:         ht = HTTOP; break;
                case WindowEdge::Bottom:      ht = HTBOTTOM; break;
                case WindowEdge::Left:        ht = HTLEFT; break;
                case WindowEdge::Right:       ht = HTRIGHT; break;
                case WindowEdge::TopLeft:     ht = HTTOPLEFT; break;
                case WindowEdge::TopRight:    ht = HTTOPRIGHT; break;
                case WindowEdge::BottomLeft:  ht = HTBOTTOMLEFT; break;
                case WindowEdge::BottomRight: ht = HTBOTTOMRIGHT; break;
            }

            SendMessage(_hWndWindow, WM_NCLBUTTONDOWN, ht, 0);
            break;
        }
        case WM_SETCURSOR: {
            HCURSOR hCursor;

            if (edgeToCursor.contains(edge)) {
                hCursor = edgeToCursor[edge];
            } else {
                LPWSTR cursor = nullptr;
                switch (edge) {
                    case WindowEdge::Top:
                    case WindowEdge::Bottom:
                        cursor = IDC_SIZENS;
                        break;
                    case WindowEdge::Left:
                    case WindowEdge::Right:
                        cursor = IDC_SIZEWE;
                        break;
                    case WindowEdge::TopLeft:
                    case WindowEdge::BottomRight:
                        cursor = IDC_SIZENWSE;
                        break;
                    case WindowEdge::TopRight:
                    case WindowEdge::BottomLeft:
                        cursor = IDC_SIZENESW;
                        break;
                }

                hCursor = LoadCursor(nullptr, cursor);
                edgeToCursor[edge] = hCursor;
            }

            SetCursor(hCursor);
            return TRUE;
        }
        default: break;
    }

    return CallWindowProc(OriginalWndProc, hWnd, msg, wParam, lParam);
}

LRESULT WindowFrame::WndFrameProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    static std::unordered_map<WindowEdge, HCURSOR> edgeToCursor;

    if (StaticHandleToInfo.contains(hWnd)) {
        const auto& [edge, _, frame] = StaticHandleToInfo[hWnd];
        return frame->WndFrameEdgeProc(hWnd, edge, msg, wParam, lParam);
    }

    return CallWindowProc(OriginalWndProc, hWnd, msg, wParam, lParam);
}

