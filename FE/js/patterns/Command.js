class ICommand {
    execute() { throw new Error("Method execute() must be implemented."); }
    undo() { throw new Error("Method undo() must be implemented."); }
}

class ChangeReadingModeCommand extends ICommand {
    constructor(readerContext, newSettings, oldSettings) {
        super();
        this.readerContext = readerContext;
        this.newSettings = newSettings;
        this.oldSettings = oldSettings;
    }
    execute() { 
        if (this.newSettings.mode) this.readerContext.setReadingMode(this.newSettings.mode);
        if (this.newSettings.fontSize) this.readerContext.setFontSize(this.newSettings.fontSize);
        if (this.newSettings.fontFamily) this.readerContext.setFontFamily(this.newSettings.fontFamily);
        if (this.newSettings.lineHeight) this.readerContext.setLineHeight(this.newSettings.lineHeight);
    }
    undo() { 
        if (this.oldSettings.mode) this.readerContext.setReadingMode(this.oldSettings.mode);
        if (this.oldSettings.fontSize) this.readerContext.setFontSize(this.oldSettings.fontSize);
        if (this.oldSettings.fontFamily) this.readerContext.setFontFamily(this.oldSettings.fontFamily);
        if (this.oldSettings.lineHeight) this.readerContext.setLineHeight(this.oldSettings.lineHeight);
    }
}

class SettingsInvoker {
    constructor() {
        this.undoStack = [];
        this.redoStack = [];
    }

    executeCommand(command) {
        command.execute();
        this.undoStack.push(command);
        this.redoStack = []; 
        this.saveToLocal();
    }

    undo() {
        if (this.undoStack.length > 0) {
            const command = this.undoStack.pop();
            command.undo();
            this.redoStack.push(command);
            this.saveToLocal();
            return command.oldSettings;
        }
        return null;
    }

    redo() {
        if (this.redoStack.length > 0) {
            const command = this.redoStack.pop();
            command.execute();
            this.undoStack.push(command);
            this.saveToLocal();
            return command.newSettings;
        }
        return null;
    }

    canUndo() { return this.undoStack.length > 0; }
    canRedo() { return this.redoStack.length > 0; }

    clearHistory() {
        this.undoStack = [];
        this.redoStack = [];
        this.saveToLocal();
    }

    saveToLocal() {
        try {
            // Bọc try-catch để chống Crash khi chạy offline (file:///)
            localStorage.setItem('settings_history', JSON.stringify({
                undos: this.undoStack.length,
                redos: this.redoStack.length
            }));
        } catch (error) {
            console.warn("Cảnh báo: LocalStorage bị chặn (do chạy offline). Chức năng History sẽ lưu trên RAM.");
        }
    }
}