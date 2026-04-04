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
