#include "msgbox.h"

#include <Windows.h>

using namespace Gluino;

DialogResult MsgBox::Show(const nwnd owner, const cstr text, const cstr title, const MessageBoxButtons buttons, const MessageBoxIcon icon) {
    UINT flags = {};

    switch (buttons) {
        case MessageBoxButtons::OK:                flags |= MB_OK; break;
        case MessageBoxButtons::OKCancel:          flags |= MB_OKCANCEL; break;
        case MessageBoxButtons::AbortRetryIgnore:  flags |= MB_ABORTRETRYIGNORE; break;
        case MessageBoxButtons::YesNoCancel:       flags |= MB_YESNOCANCEL; break;
        case MessageBoxButtons::YesNo:             flags |= MB_YESNO; break;
        case MessageBoxButtons::RetryCancel:       flags |= MB_RETRYCANCEL; break;
        case MessageBoxButtons::CancelTryContinue: flags |= MB_CANCELTRYCONTINUE; break;
    }

    switch (icon) {
        case MessageBoxIcon::None:        break;
        case MessageBoxIcon::Error:       flags |= MB_ICONERROR; break;
        case MessageBoxIcon::Warning:     flags |= MB_ICONWARNING; break;
        case MessageBoxIcon::Information: flags |= MB_ICONINFORMATION; break;
    }

    switch (MessageBoxW(owner, text, title, flags)) {
        case IDOK:       return DialogResult::OK;
        case IDCANCEL:   return DialogResult::Cancel;
        case IDABORT:    return DialogResult::Abort;
        case IDRETRY:    return DialogResult::Retry;
        case IDIGNORE:   return DialogResult::Ignore;
        case IDYES:      return DialogResult::Yes;
        case IDNO:       return DialogResult::No;
        case IDTRYAGAIN: return DialogResult::TryAgain;
        case IDCONTINUE: return DialogResult::Continue;
        default:         return DialogResult::None;
    }
}
