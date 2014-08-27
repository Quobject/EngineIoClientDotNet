module.exports = function (grunt) {

  grunt.registerTask('testClient',
      'test cs', function () {
    var
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      test_format_str = os === 'win' ?
        '{0}/xunit.console.clr4.exe {1} /nunit test.xml' :
        'mono {0}/xunit.console.clr4.exe {1}',
      xunit_path = os === 'win' ?
        config.win.xunit_path : config.linux.xunit_path,    
      testdll = __dirname + '/../../EngineIoClientDotNet_Tests/bin/Release/EngineIoClientDotNet_Tests.dll';


    grunt.log.writeln('testdll = "%s"', testdll);

    tasks.push(  string.format(test_format_str,xunit_path, testdll) );

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


