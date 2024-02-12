#pragma once

#ifndef GLUINO_FILEDIALOG_INFO_H
#define GLUINO_FILEDIALOG_INFO_H

#include "common.h"

namespace Gluino {

struct FileDialogInfo {
    cstr Title;
    cstr DefaultPath;
    bool MultiSelect;
    FileDialogFilter* Filters;
    int FilterCount;
    mutable cstr* FileNames;
    mutable int FileNameCount;
};

}

#endif // !GLUINO_FILEDIALOG_INFO_H
