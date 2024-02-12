#pragma once

#ifndef GLUINO_MESSAGEBOX_BASE_H
#define GLUINO_MESSAGEBOX_BASE_H

#include "common.h"

namespace Gluino {

template<typename Derived>
class MsgBoxBase {
public:
    static DialogResult Show(nwnd owner, cstr text, cstr title, MessageBoxButtons buttons, MessageBoxIcon icon) {
        return Derived::Show(owner, text, title, buttons, icon);
    }
};

}

#endif // !GLUINO_MESSAGEBOX_BASE_H
