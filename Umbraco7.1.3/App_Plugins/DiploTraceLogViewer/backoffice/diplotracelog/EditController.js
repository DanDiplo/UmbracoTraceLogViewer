'use strict';
app.requires.push('smart-table');

angular.module("umbraco").controller("DiploTraceLogEditController",
    function ($scope, $http, $routeParams, $route, $filter, $q, $templateCache, $timeout, $window, dialogService, stConfig, diploTraceLogResources) {
        var timer;
        var lastModified = 0;
        var persistKey = "diploTraceLogPersist";
        var pollingOnText = "Polling ";
        var pollingOffText = "Polling Off";
        var pollingIndicatorChar = "▪";

        $scope.isLoading = true;
        $scope.pageSize = {};
        $scope.persist = localStorage.getItem(persistKey) === "true" || false;

        $scope.polling = {
            enabled: false,
            interval: 5,
            buttonText: pollingOffText,
            indicator: ""
        };

        $scope.id = $routeParams.id;

        $scope.levelOptions = [
            { label: "Info", value: "INFO" },
            { label: "Warn", value: "WARN" },
            { label: "Error", value: "ERROR" }
        ];

        $scope.pollOptions = [
            { label: "Disabled", value: 0 },
            { label: "1 Second", value: 1 },
            { label: "5 Seconds", value: 5 },
            { label: "10 Seconds", value: 10 },
            { label: "60 Seconds", value: 60 },
        ];

        $scope.itemsPerPage = [20, 50, 100, 200, 500, 1000];

        $scope.isCurrentLog = $routeParams.id.endsWith('.txt');

        var getLogData = function () {
            diploTraceLogResources.getLogDataResponse($routeParams.id).then(function (data) {

                $scope.rowCollection = data.LogDataItems;
                lastModified = data.LastModifiedTicks;
                $scope.isLoading = false;
            });
        }

        // Open detail modal
        $scope.openDetail = function (logItem, data) {

            var dialog = dialogService.open({
                template: '/App_Plugins/DiploTraceLogViewer/backoffice/diplotracelog/detail.html',
                dialogData: { logItem: logItem, items: data }, show: true, width: 800
            });
        }

        var tick = function () {

            $scope.polling.indicator += pollingIndicatorChar;

            if ($scope.polling.indicator.length > 3) {
                $scope.polling.indicator = "";
            }

            if ($scope.polling.interval > 0) {

                diploTraceLogResources.getLastModifiedTime($routeParams.id).then(function (data) {
                    var modified = parseInt(data);

                    if (modified > lastModified) {
                        lastModified = modified;
                        console.log("Log file has changed!");
                        getLogData();
                    }

                    timer = $timeout(tick, parseInt($scope.polling.interval) * 1000);
                });
            }
        }

        getLogData();

        var cancelTimer = function () {
            if (timer) {
                $timeout.cancel(timer);
                timer = null;
                console.log("Polling cancelled...");
            }
        }

        var startTimer = function () {

            if ($scope.isCurrentLog) {
                cancelTimer();
                $scope.polling.enabled = true;

                timer = $timeout(tick, parseInt($scope.polling.interval) * 1000);
                console.log("Polling started...");
                $scope.polling.indicator = pollingIndicatorChar;
            }
        }

        $scope.checkTimer = function () {

            if ($scope.polling.enabled) {
                startTimer();
            }
            else {
                cancelTimer();
                $scope.polling.indicator = "";
            }

            $scope.polling.buttonText = $scope.polling.enabled ? pollingOnText + " " + $scope.polling.interval + "s" : pollingOffText;
        }

        $scope.setPollInterval = function (seconds) {
            $scope.polling.interval = seconds;
            $scope.polling.enabled = true;
            $scope.checkTimer();
        }

        $scope.togglePolling = function () {
            $scope.polling.enabled = !$scope.polling.enabled;
            $scope.checkTimer();
        }

        $scope.reload = function (clear) {
            $route.reload();
            if (clear === true) {
                localStorage.removeItem("diploTraceLogTable");
                $scope.changePersist(false);
            }
        }

        $window.onblur = function () {
            cancelTimer();
        };

        $window.onfocus = function () {
            $scope.checkTimer();
        };

        $scope.changePersist = function (persist) {
            localStorage.setItem(persistKey, persist);
        }

        $scope.$on("$destroy", function (event) {
            cancelTimer();
        });

    });

app.directive('stPersist', function () {
    return {
        require: '^stTable',
        link: function (scope, element, attr, ctrl) {
            var nameSpace = attr.stPersist;

            //save the table state every time it changes
            scope.$watch(function () {
                return ctrl.tableState();
            }, function (newValue, oldValue) {
                if (newValue !== oldValue) {
                    localStorage.setItem(nameSpace, JSON.stringify(newValue));
                }
            }, true);

            //fetch the table state when the directive is loaded
            if (scope.persist && localStorage.getItem(nameSpace)) {
                var savedState = JSON.parse(localStorage.getItem(nameSpace));
                var tableState = ctrl.tableState();

                scope.pageSize = savedState.pagination.number;

                angular.extend(tableState, savedState);
                ctrl.pipe();
            }
            else if (!scope.persist) {
                ctrl.tableState().sort = { "predicate": "Date", "reverse": true };
                scope.pageSize = 100;
            }
        }
    };
});

app.directive("stResetSearch", function () {
    return {
        restrict: 'EA',
        require: '^stTable',
        link: function (scope, element, attrs, ctrl) {
            return element.bind('click', function () {
                return scope.$apply(function () {
                    var tableState;
                    tableState = ctrl.tableState();
                    tableState.search.predicateObject = {};
                    tableState.pagination.start = 0;
                    return ctrl.pipe();
                });
            });
        }
    };
})

app.directive('diploClearable', function () {
    return {
        restrict: 'E',
        require: '^stTable',
        template: '<i class="icon icon-delete"></i>',
        link: function (scope, element, attrs, ctrl) {
            return element.bind('click', function () {
                return scope.$apply(function () {

                    var name = element.next().attr('st-search');
                    var params = ctrl.tableState().search.predicateObject;

                    if (params && params[name] !== undefined) {
                        params[name] = '';
                    }

                    return ctrl.pipe();
                });

            });
        }
    }
});

app.filter('diploLastWordHighlight', function () {
    return function (input) {
        var items = input.split(".");
        var last = items.pop();
        return "<small>" + items.join(".") + "</small>." + last;
    }
});