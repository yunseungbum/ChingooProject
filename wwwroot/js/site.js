// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-history-action]").forEach((button) => {
        button.addEventListener("click", () => {
            const action = button.getAttribute("data-history-action");

            if (action === "back") {
                window.history.back();
                return;
            }

            if (action === "forward") {
                window.history.forward();
            }
        });
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const slideContainer = document.getElementById("worldcup-schedule-slide");
    const slideCount = document.getElementById("worldcup-slide-count");
    const prevButton = document.getElementById("worldcup-prev");
    const nextButton = document.getElementById("worldcup-next");

    if (!slideContainer || !slideCount) {
        return;
    }

    fetch("/api/football-data/world-cup/matches?season=2026")
        .then((response) => {
            if (!response.ok) {
                throw new Error("World Cup matches request failed.");
            }

            return response.json();
        })
        .then((data) => {
            const finishedMatches = Array.isArray(data.finishedMatches) ? data.finishedMatches : [];
            const upcomingMatches = Array.isArray(data.upcomingMatches) ? data.upcomingMatches : [];
            const sortedMatches = finishedMatches
                .concat(upcomingMatches)
                .sort((first, second) => new Date(first.koreaDate) - new Date(second.koreaDate));
            const matchGroups = groupWorldCupMatchesByDate(sortedMatches);

            startWorldCupSchedule(slideContainer, slideCount, prevButton, nextButton, matchGroups);
        })
        .catch(() => {
            slideCount.textContent = "-";
            slideContainer.innerHTML = '<div class="worldcup-empty">\uC77C\uC815\uC744 \uBD88\uB7EC\uC624\uC9C0 \uBABB\uD588\uC2B5\uB2C8\uB2E4.</div>';
            setWorldCupControlsDisabled(prevButton, nextButton, true);
        });
});

document.addEventListener("DOMContentLoaded", () => {
    const slideContainer = document.getElementById("youtube-video-slide");
    const slideCount = document.getElementById("youtube-slide-count");
    const prevButton = document.getElementById("youtube-prev");
    const nextButton = document.getElementById("youtube-next");

    if (!slideContainer || !slideCount) {
        return;
    }

    startYoutubeVideoSlider(slideContainer, slideCount, prevButton, nextButton);
});

function startYoutubeVideoSlider(container, counter, prevButton, nextButton) {
    const videos = Array.from(container.querySelectorAll(".youtube-video-item"));

    if (!videos.length) {
        counter.textContent = "0/0";
        setWorldCupControlsDisabled(prevButton, nextButton, true);
        return;
    }

    setWorldCupControlsDisabled(prevButton, nextButton, videos.length <= 1);

    let currentIndex = 0;
    let autoSlideTimer = null;
    let resumeTimer = null;

    const renderCurrentVideo = () => {
        videos.forEach((video, index) => {
            video.classList.toggle("is-active", index === currentIndex);
        });
        counter.textContent = `${currentIndex + 1}/${videos.length}`;
    };

    const showNextVideo = () => {
        currentIndex = currentIndex + 1 >= videos.length ? 0 : currentIndex + 1;
        renderCurrentVideo();
    };

    const showPreviousVideo = () => {
        currentIndex = currentIndex - 1 < 0 ? videos.length - 1 : currentIndex - 1;
        renderCurrentVideo();
    };

    const startAutoSlide = () => {
        window.clearInterval(autoSlideTimer);
        autoSlideTimer = window.setInterval(showNextVideo, 5000);
    };

    const pauseThenResumeAutoSlide = () => {
        window.clearInterval(autoSlideTimer);
        window.clearTimeout(resumeTimer);
        resumeTimer = window.setTimeout(startAutoSlide, 2000);
    };

    prevButton?.addEventListener("click", () => {
        showPreviousVideo();
        pauseThenResumeAutoSlide();
    });

    nextButton?.addEventListener("click", () => {
        showNextVideo();
        pauseThenResumeAutoSlide();
    });

    renderCurrentVideo();
    startAutoSlide();
}

function groupWorldCupMatchesByDate(matches) {
    return matches.reduce((groups, match) => {
        const dateKey = match.dateText || formatWorldCupDate(match.koreaDate);
        const existingGroup = groups.find((group) => group.dateText === dateKey);

        if (existingGroup) {
            existingGroup.matches.push(match);
        } else {
            groups.push({
                dateText: dateKey,
                matches: [match]
            });
        }

        return groups;
    }, []);
}

function formatWorldCupDate(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${month}.${day}`;
}

function startWorldCupSchedule(container, counter, prevButton, nextButton, groups) {
    if (!groups.length) {
        counter.textContent = "0/0";
        container.innerHTML = '<div class="worldcup-empty">\uD45C\uC2DC\uD560 \uC6D4\uB4DC\uCEF5 \uC77C\uC815\uC774 \uC5C6\uC2B5\uB2C8\uB2E4.</div>';
        setWorldCupControlsDisabled(prevButton, nextButton, true);
        return;
    }

    setWorldCupControlsDisabled(prevButton, nextButton, groups.length <= 1);

    let currentIndex = 0;
    let autoSlideTimer = null;
    let resumeTimer = null;

    const renderCurrentGroup = () => {
        const group = groups[currentIndex];
        container.innerHTML = createWorldCupGroupHtml(group);
        counter.textContent = `${currentIndex + 1}/${groups.length}`;
    };

    const showNextGroup = () => {
        currentIndex = currentIndex + 1 >= groups.length ? 0 : currentIndex + 1;
        renderCurrentGroup();
    };

    const showPreviousGroup = () => {
        currentIndex = currentIndex - 1 < 0 ? groups.length - 1 : currentIndex - 1;
        renderCurrentGroup();
    };

    const startAutoSlide = () => {
        window.clearInterval(autoSlideTimer);
        autoSlideTimer = window.setInterval(showNextGroup, 5000);
    };

    const pauseThenResumeAutoSlide = () => {
        window.clearInterval(autoSlideTimer);
        window.clearTimeout(resumeTimer);
        resumeTimer = window.setTimeout(startAutoSlide, 2000);
    };

    prevButton?.addEventListener("click", () => {
        showPreviousGroup();
        pauseThenResumeAutoSlide();
    });

    nextButton?.addEventListener("click", () => {
        showNextGroup();
        pauseThenResumeAutoSlide();
    });

    renderCurrentGroup();
    startAutoSlide();
}

function setWorldCupControlsDisabled(prevButton, nextButton, disabled) {
    if (prevButton) {
        prevButton.disabled = disabled;
    }

    if (nextButton) {
        nextButton.disabled = disabled;
    }
}

function createWorldCupGroupHtml(group) {
    const matchCountText = group.matches.length > 1 ? ` \u00B7 ${group.matches.length}\uACBD\uAE30` : "";

    return `
        <div class="worldcup-date-slide">
            <div class="worldcup-date-heading">
                <span>${escapeHtml(group.dateText)}</span>
                <strong>${escapeHtml(matchCountText)}</strong>
            </div>
            <div class="worldcup-date-list">
                ${group.matches.map(createWorldCupMatchHtml).join("")}
            </div>
        </div>`;
}

function createWorldCupMatchHtml(match) {
    const statusText = match.statusText || match.status || "";
    const stageText = match.stage || "";
    const scoreText = getWorldCupScoreText(match);
    const matchMeta = [match.timeText, stageText, statusText].filter(Boolean).join(" \u00B7 ");

    return `
        <div class="worldcup-date-match">
            <div class="worldcup-match-meta">${escapeHtml(matchMeta)}</div>
            <div class="worldcup-versus">
                <div class="worldcup-team worldcup-home">
                    ${createCrestHtml(match.homeTeamCrest, match.homeTeamName)}
                    <span>${escapeHtml(match.homeTeamName ?? "TBD")}</span>
                </div>

                <div class="worldcup-score">${escapeHtml(scoreText)}</div>

                <div class="worldcup-team worldcup-away">
                    <span>${escapeHtml(match.awayTeamName ?? "TBD")}</span>
                    ${createCrestHtml(match.awayTeamCrest, match.awayTeamName)}
                </div>
            </div>
        </div>`;
}

function getWorldCupScoreText(match) {
    const hasHomeScore = Number.isInteger(match.homeScore);
    const hasAwayScore = Number.isInteger(match.awayScore);

    if (hasHomeScore && hasAwayScore) {
        return `${match.homeScore}:${match.awayScore}`;
    }

    return "0:0";
}

function createCrestHtml(src, teamName) {
    if (!src) {
        return "";
    }

    return `<img src="${escapeAttribute(src)}" alt="${escapeAttribute(teamName ?? "")}" loading="lazy">`;
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function escapeAttribute(value) {
    return escapeHtml(value).replaceAll("`", "&#096;");
}
