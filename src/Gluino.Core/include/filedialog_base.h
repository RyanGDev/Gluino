#pragma once

#ifndef GLUINO_FILEDIALOG_BASE_H
#define GLUINO_FILEDIALOG_BASE_H

#include "filedialog_info.h"

namespace Gluino {

class FileDialogBase {
public:
    explicit FileDialogBase(const FileDialogType type) {
	    _type = type;
    }

    virtual ~FileDialogBase() = default;

    virtual DialogResult ShowDialog(nwnd owner, FileDialogInfo* info) = 0;

protected:
    FileDialogType _type;
};

}

#endif // !GLUINO_FILEDIALOG_BASE_H
