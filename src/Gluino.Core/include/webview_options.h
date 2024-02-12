#pragma once

#ifndef GLUINO_WEBVIEW_OPTIONS_H
#define GLUINO_WEBVIEW_OPTIONS_H

#include "common.h"

namespace Gluino {

struct WebViewOptions {
    cstr UserDataPath;
    cstr StartUrl;
    cstr StartContent;
    bool ContextMenuEnabled;
    bool DevToolsEnabled;
    cstr UserAgent;
    bool GrantPermissions;
};

}

#endif // !GLUINO_WEBVIEW_OPTIONS_H
