﻿import { createContext, useEffect, useState } from 'react'
import { QueryClient, useQuery, UseQueryResult } from '@tanstack/react-query'
import { Outlet, useNavigate, useLocation, useSearchParams } from 'react-router-dom'

import Select from 'react-select'
import { Tabset, Tab } from 'components/Tabset'
import { PageContainer } from 'styles'

import { OrganizationYear, OrganizationYearDomain, OrganizationYearView, Quarter, YearView } from 'Models/OrganizationYear'

import { IdentityClaim, User } from 'utils/authentication'
import paths from 'utils/routing/paths'
import api from 'utils/api'
import { SimpleSessionView } from 'Models/Session'
import { InstructorSchoolYearView, InstructorView } from 'Models/Instructor'
import { DateOnly } from 'Models/DateOnly'

const TabSelector = ({user}: {user: User}): JSX.Element => (
  <Tabset basePath={paths.Admin.path}>
    <Tab
      path={paths.Admin.Tabs.Overview.path}
      text='Overview'
      disabled={user.claim == IdentityClaim.Teacher}
    />
    <Tab
      path={paths.Admin.Tabs.Sessions.path}
      text='Sessions'
    />
    <Tab
      path={paths.Admin.Tabs.Staff.path}
      text='Staff'
      disabled={user.claim == IdentityClaim.Teacher}
    />
    <Tab
      path={paths.Admin.Tabs.Students.path}
      text='Students'
    />
    <Tab
      path={paths.Admin.Tabs.Config.path}
      text='Config'
      disabled={user.claim == IdentityClaim.Teacher}
    />
  </Tabset>
)

interface Props {
  user: User
  breadcrumbs: JSX.Element
}

interface IOrgYearContext {
  orgYear: OrganizationYearView | undefined
  sessionsQuery: UseQueryResult<SimpleSessionView[], Error> | undefined
  instructorsQuery: UseQueryResult<InstructorSchoolYearView[], Error> | undefined
  setOrgYear: (orgYear: OrganizationYearView) => void
}

export const OrgYearContext = createContext<IOrgYearContext>({
  orgYear: undefined,
  sessionsQuery: undefined,
  instructorsQuery: undefined,
  setOrgYear: (orgYear) => {}
})

export default ({ user, breadcrumbs}: Props) => {
  const navigate = useNavigate()
  const [orgYear, setOrgYear] = useState<OrganizationYearView>()

  const sessionsQuery = useQuery<SimpleSessionView[]>({
      queryKey: [`session?orgYearGuid=${orgYear?.guid}`],
      enabled: !!orgYear?.guid,
      select: (sessions) => sessions.map(session => ({
          ...session,
          firstSessionDate: DateOnly.toLocalDate(session.firstSessionDate as unknown as DateOnly),
          lastSessionDate: DateOnly.toLocalDate(session.lastSessionDate as unknown as DateOnly)
      })) 
  })

  const instructorsQuery = useQuery<InstructorSchoolYearView[]>({
      queryKey: [`instructor?orgYearGuid=${orgYear?.guid}`],
      enabled: !!orgYear?.guid
  })

  const orgYearContextValue = { orgYear, sessionsQuery, instructorsQuery, setOrgYear }

  function handleOrgYearChange(orgYear) {
    setOrgYear(orgYear)
  }

  useEffect(() => {
    if (location.pathname === paths.Admin.path) {
      navigate(paths.Admin.Tabs.Overview.path)
    }
  }, [location.pathname])

  return (
    <PageContainer className='rounded-top-left-0'>
      <OrgYearInput value={orgYear} onChange={handleOrgYearChange} defaultOrgYearGuid={user.organizationYearGuid} />
      <div className='w-100'>
        <TabSelector user={user} />
      </div>
      <OrgYearContext.Provider value={orgYearContextValue}>
        {orgYear ? <Outlet /> : 'Loading organizations...'}
      </OrgYearContext.Provider>
    </PageContainer>
  )
}

function getOrgYear(
  orgYears: OrganizationYearView[] | undefined, 
  orgGuid: string | undefined, 
  yearGuid: string| undefined
): OrganizationYearView | undefined {
  if (!orgYears || !orgGuid || !yearGuid)
    return undefined

  return orgYears.find(oy => oy.organization.guid === orgGuid && oy.year.guid === yearGuid)
}

const OrgYearInput = ({value, onChange, defaultOrgYearGuid}): React.ReactElement => {
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams(); //this is an argument in favor of moving the api call up a level. We deffo should..
  //an input component should be disconnected from the business logic of an api fetch anyhow.

  const { isPending, error, data: orgYears, refetch } = useQuery({
    queryKey: ['orgYears'],
    queryFn: () => api.get('user/orgYear').then(res => res.data),
    select: (data: OrganizationYearDomain[]) => data.map(oy => OrganizationYear.toViewModel(oy))
  }, new QueryClient())

  function handleInputChange(
    orgYears: OrganizationYearView[] | undefined, 
    orgGuid: string | undefined, 
    yearGuid: string| undefined
  ): void {
    const selectedOrgYear: OrganizationYearView | undefined = getOrgYear(orgYears, orgGuid, yearGuid)

    if (selectedOrgYear && selectedOrgYear.guid != value.guid && location.pathname.match(/\/home\/admin\/sessions\/([A-Z]?[a-z]?[0-9]?-?)+/))
    {
      navigate(`/home/admin/sessions?oyGuid=${selectedOrgYear.guid}`)
    }

    handleOrgYearChange(selectedOrgYear)
  }

  function handleOrgYearChange(newOrgYear: OrganizationYearView | undefined) {
    if (!newOrgYear || newOrgYear.guid === value?.guid)
      return

    onChange(newOrgYear)
  }

  useEffect(() => {
    if (!value && orgYears) {

      let defaultOrgYear = searchParams.get('oyGuid') 
        ? orgYears.find(x => x.guid === searchParams.get('oyGuid')) 
        : orgYears.find(x => x.guid === defaultOrgYearGuid)

      handleOrgYearChange(defaultOrgYear)

      searchParams.delete('oyGuid')
      setSearchParams(searchParams)
    }
  }, [orgYears])

  if (isPending)
    return <span>Loading...</span>
  else if (error)
    return (
      <div>
        <div>An error occured while fetching organization years.</div>
        <button>Try again</button>
      </div>
    )

  const orgGuids: string[] = [...(new Set(orgYears.map(x => x.organization.guid)))]
  const orgs: any[] = orgGuids.map(guid => orgYears.find(oy => oy.organization.guid === guid)?.organization)
  const years: YearView[] = orgYears.filter(oy => oy.organization.guid === value?.organization.guid)
    .map(oy => oy.year)
    .sort((current, next) => {
      if (current.schoolYear == next.schoolYear && current.quarter == next.quarter) return 0;
      else if (current.schoolYear > next.schoolYear) return -1;
      else if (current.quarter > next.quarter) return -1;
      return 1; 
  })

  return (
    <div className='row mb-3'>

      <div className='col-sm-4'>
        <label>Organization</label>
        <Select 
          value={{value: value?.organization.guid, label: value?.organization.name}}
          options={orgs.map(o => ({ value: o.guid, label: o.name }))}
          onChange={(option => handleInputChange(orgYears, option?.value, value?.year.guid))}
          isDisabled={orgs.length <= 1}
        />
      </div>

      <div className='col-sm-4'>
        <label>Term</label>
        <Select 
          value={{value: value?.year.guid, label: `${value?.year.schoolYear} - ${Quarter[value?.year.quarter]}`}}
          options={years.map(y => ({ value: y.guid, label: `${y.schoolYear} - ${Quarter[y.quarter]}`}))}
          onChange={(option => handleInputChange(orgYears, value?.organization.guid, option?.value))}
        />
      </div>

    </div>
  )
}