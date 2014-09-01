module.exports = function (grunt) {

  grunt.registerTask('nuget',
      'get assemplys ', function () {
    var
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      format_str = os === 'win' ?
        '{0} restore {1}' :
        'mono --runtime=v4.0.30319 {0} restore {1}',
      nuget_path = config.win.nuget, 
      solution = __dirname + '/../../EngineIoClientDotNet.sln';

    if (os !== 'win') {
      return;
    }

    grunt.log.writeln('nuget_path = "%s"', nuget_path);

    tasks.push(  string.format(format_str,nuget_path, solution) );

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


