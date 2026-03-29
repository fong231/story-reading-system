const BACKEND_URL = "https://localhost:7210";
const urlParams = new URLSearchParams(window.location.search);
const storyId = urlParams.get('storyId');

document.addEventListener("DOMContentLoaded", async () => {
    if (!storyId) return;
    try {
        const storyRes = await fetch(`${BACKEND_URL}/api/Stories/${storyId}`);
        const story = await storyRes.json();
        document.getElementById("story-info").innerHTML = `
            <h2>${story.title}</h2><p>${story.description}</p>
        `;

        const chapRes = await fetch(`${BACKEND_URL}/api/Chapters/Story/${storyId}`);
        const chapters = await chapRes.json();
        
        document.getElementById("chapters-container").innerHTML = chapters.map(c => `
            <a href="reader.html?storyId=${storyId}&chapterId=${c.chapterId}" 
               style="display:block; padding: 10px; border-bottom: 1px solid #eee; text-decoration: none; color: #333;">
                Chương ${c.chapterNumber}: ${c.title}
            </a>
        `).join('');
    } catch (e) { console.error(e); }
});