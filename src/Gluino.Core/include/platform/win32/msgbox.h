#pragma once

#ifndef GLUINO_MESSAGEBOX_H
#define GLUINO_MESSAGEBOX_H

#include "msgbox_base.h"

namespace Gluino {

class MsgBox : public MsgBoxBase<MsgBox> {
public:
    static DialogResult Show(nwnd owner, cstr text, cstr title, MessageBoxButtons buttons, MessageBoxIcon icon);
};

}

#endif // !GLUINO_MESSAGEBOX_H
