#include "app.h"
#include "window.h"

#include <future>

using namespace Gluino;

void Register(const autostr className) {
	WNDCLASSEX wcex;
	wcex.cbSize = sizeof WNDCLASSEX;
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = App::WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = WS_EX_NOPARENTNOTIFY;
	wcex.hInstance = App::GetHInstance();
	wcex.hIcon = LoadIcon(nullptr, IDI_APPLICATION);
	wcex.hIconSm = LoadIcon(nullptr, IDI_WINLOGO);
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_WINDOW + 1);
	wcex.lpszMenuName = nullptr;
	wcex.lpszClassName = className;

	RegisterClassEx(&wcex);

	SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
}

Window::Window(WindowOptions* options, const WindowEvents* events) : WindowBase(options, events) {
	_className = CopyStr(options->ClassName);
	_title = CopyStr(options->TitleW);

	InitDarkModeSupport();
	Register(_className);

	_hWnd = CreateWindowEx(
		WS_EX_NOREDIRECTIONBITMAP,
		_className,
		_title,
		WS_OVERLAPPEDWINDOW,
		options->Location.x,
		options->Location.y,
		options->Size.width,
		options->Size.height,
		nullptr,
		nullptr,
		App::GetHInstance(),
		this
	);

	//EnableDarkMode(_hWnd, true);
	ApplyWindowStyle(_hWnd, true);
}

Window::~Window() {
	delete[] _className;
	delete[] _title;
}

LRESULT Window::WndProc(const UINT msg, const WPARAM wParam, const LPARAM lParam) {
	switch (msg) {
		case WM_ACTIVATE: {
			if (LOWORD(wParam) == WA_INACTIVE) {
				_onFocusOut();
			}
			else {
				_onFocusIn();
				return 0;
			}
			break;
		}
		case WM_SIZE: {
			Rect bounds;
			GetBounds(&bounds);
			_onSizeChanged({ bounds.width, bounds.height });
			break;
		}
		case WM_MOVE: {
			Rect bounds;
			GetBounds(&bounds);
			_onLocationChanged({ bounds.x, bounds.y });
			break;
		}
		case WM_CLOSE: {
			if (const auto cancel = _onClosing(); !cancel) {
				DestroyWindow(_hWnd);
				return 0;
			}
			break;
		}
		case WM_NCCALCSIZE: {
			if (wParam == TRUE && _style == WindowStyle::Borderless) {
				auto& [rgrc, _] = *reinterpret_cast<NCCALCSIZE_PARAMS*>(lParam);
				AdjustMaximizedClientRect(_hWnd, rgrc[0]);
				return 0;
			}
			break;
		}
		case WM_SETTINGCHANGE: {
			if (IsColorSchemeChange(lParam)) {
				SendMessageW(_hWnd, WM_THEMECHANGED, 0, 0);
			}
			break;
		}
		case WM_THEMECHANGED: {
			ApplyWindowStyle(_hWnd, IsDarkModeEnabled());
			break;
		}
		default:
			return DefWindowProc(_hWnd, msg, wParam, lParam);
	}

	return DefWindowProc(_hWnd, msg, wParam, lParam);
}

void Window::SetBorderlessStyle(const bool borderless) const {
	const auto compositionEnabled = IsCompositionEnabled();

	constexpr DWORD aeroBorderless = WS_POPUP | WS_THICKFRAME | WS_CAPTION | WS_SYSMENU | WS_MAXIMIZEBOX | WS_MINIMIZEBOX;
	constexpr DWORD basicBorderless = WS_POPUP | WS_THICKFRAME | WS_SYSMENU | WS_MAXIMIZEBOX | WS_MINIMIZEBOX;
	const DWORD borderlessStyle = compositionEnabled ? aeroBorderless : basicBorderless;

	const DWORD newStyle = borderless ? borderlessStyle : WS_OVERLAPPEDWINDOW;
	const DWORD oldStyle = GetWindowLong(_hWnd, GWL_STYLE);

	if (newStyle == oldStyle) {
		return;
	}

	SetWindowLongW(_hWnd, GWL_STYLE, static_cast<LONG>(newStyle));

	if (compositionEnabled) {
		constexpr MARGINS shadowState[2] = { {0, 0, 0, 0}, {1, 1, 1, 1} };
		DwmExtendFrameIntoClientArea(_hWnd, &shadowState[borderless]);
	}

	SetWindowPos(_hWnd, nullptr, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE);
	ShowWindow(_hWnd, SW_SHOW);
}

void Window::Show() {
	ShowWindow(_hWnd, SW_SHOWDEFAULT);
	UpdateWindow(_hWnd);

	if (_style != WindowStyle::Normal)
		SetWindowStyle(_style);
}

void Window::Hide() {
	ShowWindow(_hWnd, SW_HIDE);
}

void Window::Close() {
	PostMessage(_hWnd, WM_CLOSE, 0, 0);
}

void Window::Invoke(Delegate action) {
	std::promise<void> promise;
	const std::future<void> future = promise.get_future();

	PostMessage(_hWnd, WM_USER_INVOKE, (WPARAM)action, (LPARAM)&promise);

	future.wait();
}

void Window::GetBounds(Rect* bounds) {
	RECT rect;
	GetWindowRect(_hWnd, &rect);

	*bounds = {
		rect.right - rect.left,
		rect.bottom - rect.top,
		rect.left,
		rect.top,
		rect.top,
		rect.right,
		rect.bottom,
		rect.left
	};
}

autostr Window::GetTitle() {
	const auto length = GetWindowTextLength(_hWnd);
	const auto title = new wchar_t[length + 1];
	GetWindowText(_hWnd, title, length + 1);
	return title;
}

void Window::SetTitle(const autostr title) {
	_title = CopyStr(title);

	SetWindowText(_hWnd, _title);
}

WindowStyle Window::GetWindowStyle() {
	return _style;
}

void Window::SetWindowStyle(WindowStyle style) {
	if (style == WindowStyle::Borderless)
		SetBorderlessStyle(true);
	else if (_style == WindowStyle::Borderless && style != WindowStyle::Borderless)
		SetBorderlessStyle(false);
	else if (style == WindowStyle::None) {
		//TODO
	}

	_style = style;	
}

WindowState Window::GetWindowState() {
	WINDOWPLACEMENT placement;
	if (GetWindowPlacement(_hWnd, &placement))
		return WindowState::Normal;
	switch (placement.showCmd) {
		case SW_MAXIMIZE:
			return WindowState::Maximized;
		case SW_MINIMIZE:
			return WindowState::Minimized;
		default:
			return WindowState::Normal;
	}
}

void Window::SetWindowState(WindowState state) {
	WINDOWPLACEMENT placement;
	if (GetWindowPlacement(_hWnd, &placement))
		return;
	switch (state) {  // NOLINT(clang-diagnostic-switch-enum)
		case WindowState::Maximized:
			placement.showCmd = SW_MAXIMIZE;
			break;
		case WindowState::Minimized:
			placement.showCmd = SW_MINIMIZE;
			break;
		default:
			placement.showCmd = SW_NORMAL;
			break;
	}
	SetWindowPlacement(_hWnd, &placement);
}
