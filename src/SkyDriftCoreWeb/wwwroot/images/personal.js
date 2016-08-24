$(document).ready(function() {
    var USER_API = 'https://api.web-skydrift.com/users/info_from_uuid/';
    var ACTIVITY_COUNT_API = 'https://ranking.web-skydrift.com/statistics/user_activity_count/';
    var WIN_POINT_API = 'https://ranking.web-skydrift.com/statistics/user_win_point/';
    var WIN_RATE_API = 'https://ranking.web-skydrift.com/statistics/user_win_rate/';
    var CHARACTERS = ['霊夢', '魔理沙', '咲夜', 'レミリア', '早苗', '諏訪子', 'こいし', 'こころ', '妖夢', '鈴仙', 'ぬえ', '布都', 'チルノ', '正邪', '萃香', '華扇', '天子', '紫'];
    var CHARACTER_TOTAL = CHARACTERS.length;
    var COURSE_TOTAL = 14;

    var getPageParams = function() {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for(var i = 0; i < hashes.length; i++)
        {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    };
    var uuid = getPageParams().key;

    var ajax = function(url, data, callback) {
        $.ajax({
            url: url,
            type: 'GET',
            data: data,
            timeout: 10000,
            error: function(xhr, status, error){
                alert('データの取得に失敗しました');
            },
            success: function(res, status, xhr){
                callback(res);
            }
        });
    };

    var fetchWinPoint = function(callback) {
        ajax(WIN_POINT_API + uuid, {}, callback);
    };
    var renderWinGraph = function() {
        fetchWinPoint(function(response) {
            var winData = [];
            for (var i=0; i<CHARACTER_TOTAL; ++i) {
                winData.push([CHARACTERS[i], response[i]]);
            }

            $("#user_win_rate").empty();
            $.jqplot(
                'user_win_rate',
                [winData],
                {
                    seriesDefaults: {
                        renderer: jQuery . jqplot . PieRenderer,
                        rendererOptions: {
                            dataLabels: 'percent',
                            showDataLabels: true,
                            startAngle: -90,
                        }
                    },
                    seriesColors: ['#f21414', '#c114f2', '#14bbf2', '#f2147d', '#1ac330', '#7bc360', '#a8b019', '#cc44c9', '#6fb49d', '#9e41e3', '#3ab8bd', '#91bcb4', '#9dd7e3', '#d42f09', '#ca9534', '#f980b5', '#55509b', '#c697e4'],
                    grid: {
                        background: "#f7f7f7",
                    },
                    legend: {
                        show: true,
                        location: 's',
                        rendererOptions: {
                            numberRows: 2
                        },
                    }
                }
            );
        });
    };
    renderWinGraph();

    var fetchUserInfo = function(callback) {
        ajax(USER_API + uuid, {}, callback);
    };
    var renderUserInfo = function() {
        fetchUserInfo(function(response) {
            var rank = response.rank;
            if (rank < 3000) {
                rank = '穏健派';
            } else if (rank < 6000) {
                rank = '中立派';
            } else {
                rank = '武闘派';
            }

            var klass = response.klass;
            if (klass === 10001) {
                klass = '殿堂入り（金）';
            } else if (klass == 10002) {
                klass = '殿堂入り（銀）';
            } else if (klass == 10003) {
                klass = '殿堂入り（銅）';
            } else if (klass < 10000) {
                klass = '妖精級';
            } else if (klass < 100000) {
                klass = '人間級';
            } else if (klass < 300000) {
                klass = '妖怪級';
            } else if (klass < 1000000) {
                klass = '神様級';
            } else if (klass < 5000000) {
                klass = '賢者級';
            } else {
                klass = '幻走級';
            }

            $('#user_klass').text(klass);
            $('#user_name').text(response.name);
            $('#user_rank').text(rank);
        });
    };
    renderUserInfo();

    var fetchWinRate = function(callback) {
        ajax(WIN_RATE_API + uuid, {}, callback);
    };
    var renderWinRate = function() {
        fetchWinRate(function(response) {
            var max = response.indexOf(Math.max.apply(null, response)) + 1;
            var min = response.indexOf(Math.min.apply(null, response)) + 1;
            var max_src = 'image/image_result/result_course_' + ('00' + max).slice(-3) + '.png';
            var min_src = 'image/image_result/result_course_' + ('00' + min).slice(-3) + '.png';
            $('#best_course').attr('src', max_src);
            $('#worst_course').attr('src', min_src);

            for (var i=0; i<COURSE_TOTAL; ++i) {
                var percent = Math.round(response[i] * 100) / 100;
                $('#course_' + i).text(percent + '%');
            }
        });
    };
    renderWinRate();

    var fetchActivityCount = function(callback) {
        ajax(ACTIVITY_COUNT_API + uuid, {}, callback);
    };
    var renderActivityRate = function() {
        fetchActivityCount(function(response) {
            var total = response.reduce(function(sum, n) {
                return sum + n;
            });
            for (var i=0; i<CHARACTER_TOTAL; ++i) {
                var percent = Math.round(response[i] * 10000 / total) / 100;
                $('#use_' + i).text(percent + '%');

                var bar_selector = '#bar_' + (('00' + (i+1)).slice(-3));
                $(bar_selector).css('height', 200 * (percent / 100.0) + 'px');
            }
        });
    };
    renderActivityRate();
});
