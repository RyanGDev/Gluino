#pragma once

#ifndef GLUINO_WINDOW_FRAME_H
#define GLUINO_WINDOW_FRAME_H

#include "common.h"

#include <Windows.h>
#include <vector>
#include <unordered_map>

namespace Gluino {

class WindowFrame;

struct WindowInfo {
    WindowEdge edge;
    nwnd parentWindow;
    WindowFrame* frame;
};

class WindowFrame {
public:
    explicit WindowFrame(nwnd parentWindow);
    ~WindowFrame();

    void Attach();
    void Detach();
    void Update() const;

private:
    nwnd _parentWindow;
    std::vector<HWND> _hWndEdges;
    bool _isAttached = false;

    std::unordered_map<HWND, WNDPROC> _oldProcs;
    std::unordered_map<HWND, WindowInfo> _infos;

    Rect GetEdgeRect(WindowEdge edge) const;

    LRESULT CALLBACK WndFrameEdgeProc(HWND hWnd, WindowEdge edge, UINT msg, WPARAM wParam, LPARAM lParam);
    static LRESULT CALLBACK WndFrameProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
};

}

#endif // !GLUINO_WINDOW_FRAME_H
