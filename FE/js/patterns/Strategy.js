// Interface ảo
class IReadingStrategy {
    apply(container) { throw new Error("Method apply() must be implemented."); }
}

class DayScrollStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.remove('theme-night');
        container.classList.remove('mode-flip');
        container.classList.add('mode-scroll');
        container.style.fontFamily = "Arial, sans-serif";
        console.log("Applied: Day Scroll Strategy");
    }
}

class NightScrollStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.add('theme-night');
        container.classList.remove('mode-flip');
        container.classList.add('mode-scroll');
        container.style.fontFamily = "Arial, sans-serif";
        console.log("Applied: Night Scroll Strategy");
    }
}

class DayFlipStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.remove('theme-night');
        container.classList.remove('mode-scroll');
        container.classList.add('mode-flip');
        container.style.fontFamily = "Georgia, serif";
        console.log("Applied: Day Flip Strategy");
    }
}

class NightFlipStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.add('theme-night');
        container.classList.remove('mode-scroll');
        container.classList.add('mode-flip');
        container.style.fontFamily = "Georgia, serif";
        console.log("Applied: Night Flip Strategy");
    }
}