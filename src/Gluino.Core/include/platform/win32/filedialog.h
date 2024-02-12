#pragma once

#ifndef GLUINO_FILEDIALOG_H
#define GLUINO_FILEDIALOG_H

#include "filedialog_base.h"

namespace Gluino {

class FileDialog final : public FileDialogBase {
public:
    explicit FileDialog(const FileDialogType type) : FileDialogBase(type) {}

    DialogResult ShowDialog(nwnd owner, FileDialogInfo* info) override;
};

}

#endif // !GLUINO_FILEDIALOG_H
